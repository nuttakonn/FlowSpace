import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { useAuthStore } from '@/store/useAuthStore';
import { RemoteUser } from '@/types/canvas';
import throttle from 'lodash/throttle';

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected' | 'error';

export interface UseCollaborationResult {
  connectionStatus: ConnectionStatus;
  remoteUsers: RemoteUser[];
  sendCursorPosition: (x: number, y: number) => void;
  sendSelectionChange: (selectedNodeIds: string[]) => void;
}

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'https://flowspace-api-1kor.onrender.com/api/v1';
const base = apiBaseUrl.replace('/api/v1', '');
const hubUrl = `${base}/hubs/collaboration`;

export function useCollaboration(boardId: string | null): UseCollaborationResult {
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('disconnected');
  const [remoteUsers, setRemoteUsers] = useState<RemoteUser[]>([]);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!boardId) {
      setConnectionStatus('disconnected');
      setRemoteUsers([]);
      return;
    }

    setConnectionStatus('connecting');

    const accessToken = useAuthStore.getState().accessToken;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken || '',
      })
      .withHubProtocol(new MessagePackHubProtocol())
      .withAutomaticReconnect()
      .build();

    connection.on('UserJoined', (user: RemoteUser) => {
      setRemoteUsers((prev) => {
        const exists = prev.some((u) => u.userId === user.userId);
        if (exists) {
          return prev.map((u) => (u.userId === user.userId ? user : u));
        }
        return [...prev, user];
      });
    });

    connection.on('UserLeft', (userId: string) => {
      setRemoteUsers((prev) => prev.filter((u) => u.userId !== userId));
    });

    connection.on('CursorMoved', (userId: string, cursor: { x: number; y: number }) => {
      setRemoteUsers((prev) =>
        prev.map((u) => (u.userId === userId ? { ...u, cursor } : u))
      );
    });

    connection.on('SelectionChanged', (userId: string, selectedNodeIds: string[]) => {
      setRemoteUsers((prev) =>
        prev.map((u) => (u.userId === userId ? { ...u, selectedNodeIds } : u))
      );
    });

    connection.onreconnecting(() => {
      setConnectionStatus('connecting');
    });

    connection.onreconnected(() => {
      setConnectionStatus('connected');
      connection.invoke('JoinBoard', boardId).catch((err) => {
        console.error('Failed to re-join board after reconnect:', err);
      });
    });

    connection.onclose(() => {
      setConnectionStatus('disconnected');
    });

    const startConnection = async (): Promise<void> => {
      try {
        await connection.start();
        setConnectionStatus('connected');
        await connection.invoke('JoinBoard', boardId);
      } catch (err) {
        setConnectionStatus('error');
        console.error('SignalR Connection Error:', err);
      }
    };

    startConnection();
    connectionRef.current = connection;

    return () => {
      connectionRef.current = null;
      const stopConnection = async (): Promise<void> => {
        if (connection.state === signalR.HubConnectionState.Connected) {
          try {
            await connection.invoke('LeaveBoard', boardId);
          } catch (err) {
            console.error('Failed to leave board:', err);
          }
        }
        try {
          await connection.stop();
        } catch (err) {
          console.error('Error stopping SignalR connection:', err);
        }
      };
      stopConnection();
    };
  }, [boardId]);

  const sendCursorPositionRaw = useCallback((x: number, y: number): void => {
    const conn = connectionRef.current;
    if (conn && conn.state === signalR.HubConnectionState.Connected) {
      conn.invoke('MoveCursor', x, y).catch((err) => {
        console.error('Failed to send cursor position:', err);
      });
    }
  }, []);

  const sendCursorPosition = useMemo(
    () => throttle(sendCursorPositionRaw, 33), // ~30 fps (1000ms / 30 = 33.3ms)
    [sendCursorPositionRaw]
  );

  const sendSelectionChange = useCallback((selectedNodeIds: string[]): void => {
    const conn = connectionRef.current;
    if (conn && conn.state === signalR.HubConnectionState.Connected) {
      conn.invoke('ChangeSelection', selectedNodeIds).catch((err) => {
        console.error('Failed to send selection change:', err);
      });
    }
  }, []);

  // Cleanup throttle on unmount
  useEffect(() => {
    return () => {
      sendCursorPosition.cancel();
    };
  }, [sendCursorPosition]);

  return {
    connectionStatus,
    remoteUsers,
    sendCursorPosition,
    sendSelectionChange,
  };
}
