"use client";

import { useCanvasStore } from "@/store/useCanvasStore";
import { useReactFlow } from "@xyflow/react";

export function RemoteSelectionHighlights() {
  const remoteUsers = useCanvasStore((state) => state.remoteUsers);
  const { getNode } = useReactFlow();

  return (
    <>
      {Object.entries(remoteUsers).map(([clientId, user]) => {
        return user.selection.map((nodeId) => {
          const node = getNode(nodeId);
          if (!node || !node.measured) return null;

          return (
            <div
              key={`${clientId}-${nodeId}`}
              className="pointer-events-none absolute z-0 rounded-sm border-2 transition-all duration-200"
              style={{
                left: node.position.x - 4,
                top: node.position.y - 4,
                width: (node.measured.width ?? 0) + 8,
                height: (node.measured.height ?? 0) + 8,
                borderColor: user.color,
                boxShadow: `0 0 0 2px ${user.color}33`,
              }}
            >
              <div 
                className="absolute -top-6 left-0 rounded px-1 py-0.5 text-[8px] font-bold text-white whitespace-nowrap"
                style={{ backgroundColor: user.color }}
              >
                {user.name} selecting
              </div>
            </div>
          );
        });
      })}
    </>
  );
}
