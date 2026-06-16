"use client";

import { useCanvasStore, RemoteUser } from "@/store/useCanvasStore";
import { MousePointer2 } from "lucide-react";

export function RemoteCursors() {
  const remoteUsers = useCanvasStore((state) => state.remoteUsers);

  return (
    <>
      {Object.entries(remoteUsers).map(([clientId, user]) => {
        if (!user.cursor) return null;

        return (
          <div
            key={clientId}
            className="pointer-events-none absolute z-50 flex flex-col items-center gap-1 transition-all duration-100 ease-out"
            style={{
              left: user.cursor.x,
              top: user.cursor.y,
              transform: "translate(-5px, -5px)",
            }}
          >
            <MousePointer2
              className="h-4 w-4"
              style={{
                fill: user.color,
                color: user.color,
              }}
            />
            <div
              className="rounded px-1.5 py-0.5 text-[10px] font-bold text-white shadow-sm"
              style={{ backgroundColor: user.color }}
            >
              {user.name}
            </div>
          </div>
        );
      })}
    </>
  );
}
