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
import { cn } from '@/lib/utils';

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

interface SelectionNodeData {
  label?: string;
  color?: string;
  fontSize?: number;
  fontFamily?: string;
  textColor?: string;
}

export function SelectionToolbar({ isVisible, nodeId }: SelectionToolbarProps) {
  const deleteElements = useCanvasStore(s => s.deleteElements);
  const updateNodeColor = useCanvasStore(s => s.updateNodeColor);
  const updateNodeTextStyle = useCanvasStore(s => s.updateNodeTextStyle);
  const nodes = useCanvasStore(s => s.nodes);

  const currentNode = nodes.find(n => n.id === nodeId);
  const nodeData = currentNode?.data as SelectionNodeData | undefined;

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
          
          <Popover>
            <PopoverTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8 hover:bg-background" title="Text Style">
                <Type className="h-4 w-4" />
              </Button>
            </PopoverTrigger>
            <PopoverContent side="top" align="center" className="w-64 p-3 space-y-3 bg-background border rounded-lg shadow-xl z-50">
              <div className="space-y-1">
                <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-wider">Font Family</span>
                <div className="grid grid-cols-3 gap-1">
                  {[
                    { name: 'Sans', value: 'sans', className: 'font-sans' },
                    { name: 'Serif', value: 'serif', className: 'font-serif' },
                    { name: 'Mono', value: 'mono', className: 'font-mono' },
                  ].map((f) => (
                    <Button
                      key={f.value}
                      variant={nodeData?.fontFamily === f.value ? 'secondary' : 'ghost'}
                      size="sm"
                      className={cn("h-7 text-xs font-semibold px-1", f.className)}
                      onClick={() => updateNodeTextStyle(nodeId, { fontFamily: f.value })}
                    >
                      {f.name}
                    </Button>
                  ))}
                </div>
              </div>

              <div className="space-y-1">
                <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-wider block">Font Size</span>
                <div className="flex items-center gap-1.5">
                  <Button 
                    variant="outline" 
                    size="icon" 
                    className="h-7 w-7" 
                    onClick={() => {
                      const currentSize = nodeData?.fontSize || 12;
                      const sizes = [12, 14, 16, 18, 20, 24, 28, 32];
                      const currentIndex = sizes.indexOf(currentSize);
                      const nextSize = currentIndex > 0 ? sizes[currentIndex - 1] : Math.max(10, currentSize - 2);
                      updateNodeTextStyle(nodeId, { fontSize: nextSize });
                    }}
                  >
                    -
                  </Button>
                  <span className="text-xs font-bold w-10 text-center">{nodeData?.fontSize || 12}px</span>
                  <Button 
                    variant="outline" 
                    size="icon" 
                    className="h-7 w-7" 
                    onClick={() => {
                      const currentSize = nodeData?.fontSize || 12;
                      const sizes = [12, 14, 16, 18, 20, 24, 28, 32];
                      const currentIndex = sizes.indexOf(currentSize);
                      const nextSize = currentIndex !== -1 && currentIndex < sizes.length - 1 ? sizes[currentIndex + 1] : Math.min(48, currentSize + 2);
                      updateNodeTextStyle(nodeId, { fontSize: nextSize });
                    }}
                  >
                    +
                  </Button>
                  <div className="flex-1 flex gap-1 justify-end">
                    {[12, 16, 20, 24].map((sz) => (
                      <button
                        key={sz}
                        onClick={() => updateNodeTextStyle(nodeId, { fontSize: sz })}
                        className={cn(
                          "px-1.5 py-0.5 rounded text-[10px] border font-semibold transition-all",
                          (nodeData?.fontSize || 12) === sz ? "bg-primary text-primary-foreground border-primary" : "bg-muted hover:bg-muted/80"
                        )}
                      >
                        {sz}
                      </button>
                    ))}
                  </div>
                </div>
              </div>

              <div className="space-y-1">
                <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-wider block">Text Color</span>
                <div className="flex flex-wrap gap-1">
                  {[
                    { name: 'Default', value: 'black', bg: 'bg-slate-900 border-slate-800' },
                    { name: 'Red', value: 'red', bg: 'bg-red-600 border-red-700' },
                    { name: 'Green', value: 'green', bg: 'bg-green-600 border-green-700' },
                    { name: 'Blue', value: 'blue', bg: 'bg-blue-600 border-blue-700' },
                    { name: 'Yellow', value: 'yellow', bg: 'bg-yellow-500 border-yellow-600' },
                    { name: 'Purple', value: 'purple', bg: 'bg-purple-600 border-purple-700' },
                    { name: 'Orange', value: 'orange', bg: 'bg-orange-500 border-orange-600' },
                    { name: 'Gray', value: 'gray', bg: 'bg-slate-500 border-slate-600' },
                  ].map((c) => (
                    <button
                      key={c.value}
                      title={c.name}
                      onClick={() => updateNodeTextStyle(nodeId, { textColor: c.value })}
                      className={cn(
                        "h-5 w-5 rounded-full border shadow-sm transition-transform hover:scale-110",
                        c.bg,
                        nodeData?.textColor === c.value && "ring-2 ring-primary ring-offset-1"
                      )}
                    />
                  ))}
                </div>
              </div>
            </PopoverContent>
          </Popover>
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
