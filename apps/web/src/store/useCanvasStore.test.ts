import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useCanvasStore } from './useCanvasStore';

describe('useCanvasStore Mutation Queue System', () => {
  beforeEach(() => {
    const store = useCanvasStore.getState();
    
    // Clear Yjs maps for clean state
    store.yNodes.clear();
    store.yEdges.clear();

    // Register observers to sync Yjs updates with Zustand state in tests
    store.yNodes.observe(() => {
      useCanvasStore.setState({
        nodes: Array.from(store.yNodes.values())
      });
    });
    store.yEdges.observe(() => {
      useCanvasStore.setState({
        edges: Array.from(store.yEdges.values())
      });
    });

    useCanvasStore.setState({
      nodes: [],
      edges: [],
      past: [],
      future: [],
      mutationQueue: [],
      syncStatus: 'idle',
      tempToRealIdMap: {},
      boardId: 'test-board'
    });
  });

  it('should optimistically add node and queue mutation', () => {
    const store = useCanvasStore.getState();
    
    // Add a node
    store.addNode('Rectangle', { x: 100, y: 100 });
    
    const updatedStore = useCanvasStore.getState();
    
    // UI should update immediately
    expect(updatedStore.nodes.length).toBe(1);
    expect(updatedStore.nodes[0].id.startsWith('temp-')).toBe(true);
    
    // Queue should have the mutation
    expect(updatedStore.mutationQueue.length).toBe(1);
    expect(updatedStore.mutationQueue[0].type).toBe('CREATE_NODE');
  });

  it('should resolve real IDs from temporary IDs', () => {
    const store = useCanvasStore.getState();
    
    // Setup mapping
    useCanvasStore.setState({
      tempToRealIdMap: {
        'temp-123': 'real-123'
      }
    });

    const resolvedId = useCanvasStore.getState().resolveRealId('temp-123');
    expect(resolvedId).toBe('real-123');

    // Should return original if not mapped
    const missingId = useCanvasStore.getState().resolveRealId('unknown-id');
    expect(missingId).toBe('unknown-id');
  });

  it('should queue node updates and deletions', () => {
    const store = useCanvasStore.getState();
    
    store.saveNodePosition({
      id: 'real-123',
      position: { x: 50, y: 50 },
      data: {}
    } as any);

    expect(useCanvasStore.getState().mutationQueue[0].type).toBe('UPDATE_NODE');

    store.deleteElements([{ id: 'real-123' } as any], []);
    
    expect(useCanvasStore.getState().mutationQueue[1].type).toBe('DELETE_NODE');
  });
});
