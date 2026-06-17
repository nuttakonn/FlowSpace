"use client";

import React from 'react';
import { NodeToolbar, Position } from '@xyflow/react';
import { 
  Trash2, 
  Copy, 
  ArrowUpRight, 
  Palette,
  Type,
  MoreVertical
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
  DropdownMenuLabel
} from "@/components/ui/dropdown-menu";
import { useCanvasStore } from '@/store/useCanvasStore';

interface SelectionToolbarProps {
  isVisible: boolean;
  nodeId: string;
}

export function SelectionToolbar({ isVisible, nodeId }: SelectionToolbarProps) {
  const deleteElements = useCanvasStore(s => s.deleteElements);
  const nodes = useCanvasStore(s => s.nodes);

  const handleDelete = () => {
    const node = nodes.find(n => n.id === nodeId);
    if (node) deleteElements([node], []);
  };

  if (!isVisible) return null;

  return (
    <NodeToolbar 
      isVisible={isVisible} 
      position={Position.Top}
      className="flex items-center gap-1 p-1 bg-background border rounded-lg shadow-xl"
    >
      <div className="flex items-center bg-muted/20 rounded p-0.5 gap-0.5">
          <Button variant="ghost" size="icon" className="h-8 w-8 hover:bg-background" title="Change Color">
            <Palette className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 hover:bg-background" title="Text Style">
            <Type className="h-4 w-4" />
          </Button>
      </div>

      <div className="w-px h-4 bg-border mx-1" />

      <Button variant="ghost" size="icon" className="h-8 w-8" title="Add Connector">
        <ArrowUpRight className="h-4 w-4" />
      </Button>
      
      <Button variant="ghost" size="icon" className="h-8 w-8" title="Duplicate">
        <Copy className="h-4 w-4" />
      </Button>

      <div className="w-px h-4 bg-border mx-1" />

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="icon" className="h-8 w-8">
            <MoreVertical className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-40">
          <DropdownMenuLabel>Actions</DropdownMenuLabel>
          <DropdownMenuItem>Bring to Front</DropdownMenuItem>
          <DropdownMenuItem>Send to Back</DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuItem className="text-destructive focus:text-destructive" onClick={handleDelete}>
            <Trash2 className="mr-2 h-4 w-4" />
            Delete
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </NodeToolbar>
  );
}
