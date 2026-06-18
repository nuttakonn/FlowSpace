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
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { useCanvasStore } from '@/store/useCanvasStore';

interface SelectionToolbarProps {
  isVisible: boolean;
  nodeId: string;
}

const COLORS = [
  { label: 'Default', value: '', dot: 'bg-primary border-primary' },
  { label: 'Red', value: 'bg-red-50/90 border-red-400 text-red-900', dot: 'bg-red-400 border-red-500' },
  { label: 'Green', value: 'bg-green-50/90 border-green-400 text-green-900', dot: 'bg-green-400 border-green-500' },
  { label: 'Yellow', value: 'bg-yellow-50/90 border-yellow-400 text-yellow-900', dot: 'bg-yellow-400 border-yellow-500' },
  { label: 'Blue', value: 'bg-blue-50/90 border-blue-400 text-blue-900', dot: 'bg-blue-400 border-blue-500' },
  { label: 'Purple', value: 'bg-purple-50/90 border-purple-400 text-purple-900', dot: 'bg-purple-400 border-purple-500' },
  { label: 'Orange', value: 'bg-orange-50/90 border-orange-400 text-orange-900', dot: 'bg-orange-400 border-orange-500' },
  { label: 'Gray', value: 'bg-slate-50/90 border-slate-400 text-slate-900', dot: 'bg-slate-400 border-slate-500' },
];

export function SelectionToolbar({ isVisible, nodeId }: SelectionToolbarProps) {
  const deleteElements = useCanvasStore(s => s.deleteElements);
  const updateNodeColor = useCanvasStore(s => s.updateNodeColor);
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
          <Popover>
            <PopoverTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8 hover:bg-background" title="Change Color">
                <Palette className="h-4 w-4" />
              </Button>
            </PopoverTrigger>
            <PopoverContent side="top" align="center" className="w-48 p-2 flex flex-wrap gap-1.5 justify-center">
              {COLORS.map((c) => (
                <button
                  key={c.label}
                  title={c.label}
                  onClick={() => updateNodeColor(nodeId, c.value)}
                  className={`h-6 w-6 rounded-full border shadow-sm transition-transform hover:scale-110 flex items-center justify-center ${c.dot}`}
                />
              ))}
            </PopoverContent>
          </Popover>
          
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
