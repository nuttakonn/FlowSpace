"use client";

import React, { useState } from 'react';
import { 
  MousePointer2, 
  Square, 
  Circle, 
  Diamond, 
  Database, 
  Cloud, 
  Type, 
  StickyNote, 
  ArrowUpRight,
  Plus,
  LucideIcon
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from '@/lib/utils';

interface ToolbarButtonProps {
  icon: LucideIcon;
  label: string;
  onClick?: () => void;
  isActive?: boolean;
  className?: string;
}

const ToolbarButton = ({ icon: Icon, label, onClick, isActive, className }: ToolbarButtonProps) => (
  <TooltipProvider delayDuration={300}>
    <Tooltip>
      <TooltipTrigger asChild>
        <Button
          variant={isActive ? "secondary" : "ghost"}
          size="icon"
          className={cn("h-10 w-10 rounded-lg", isActive && "bg-primary/10 text-primary", className)}
          onClick={onClick}
        >
          <Icon className="h-5 w-5" />
        </Button>
      </TooltipTrigger>
      <TooltipContent side="right">
        <p>{label}</p>
      </TooltipContent>
    </Tooltip>
  </TooltipProvider>
);

interface FloatingToolbarProps {
  onAddNode: (type: string) => void;
  className?: string;
}

export function FloatingToolbar({ onAddNode, className }: FloatingToolbarProps) {
  const [activeTool, setActiveTool] = useState('select');

  const shapes = [
    { type: 'Rectangle', icon: Square, label: 'Rectangle' },
    { type: 'Circle', icon: Circle, label: 'Circle' },
    { type: 'Diamond', icon: Diamond, label: 'Diamond' },
    { type: 'Database', icon: Database, label: 'Database' },
    { type: 'Cloud', icon: Cloud, label: 'Cloud' },
  ];

  return (
    <div className={cn("flex flex-col gap-2 p-2 bg-background/80 backdrop-blur border rounded-xl shadow-lg", className)}>
      <ToolbarButton 
        icon={MousePointer2} 
        label="Select (V)" 
        isActive={activeTool === 'select'} 
        onClick={() => setActiveTool('select')} 
      />
      
      <div className="h-px bg-border mx-2" />

      <Popover>
        <PopoverTrigger asChild>
          <div className="relative">
            <ToolbarButton 
              icon={Plus} 
              label="Add Shape" 
              isActive={['Rectangle', 'Circle', 'Diamond', 'Database', 'Cloud'].includes(activeTool)}
            />
            <div className="absolute bottom-1 right-1 h-2 w-2 bg-primary rounded-full scale-0 transition-transform" />
          </div>
        </PopoverTrigger>
        <PopoverContent side="right" align="start" className="w-48 p-2 flex flex-col gap-1">
          <p className="text-[10px] font-bold text-muted-foreground px-2 py-1 uppercase tracking-wider">Shapes</p>
          {shapes.map((shape) => (
            <Button
              key={shape.type}
              variant="ghost"
              className="justify-start h-9 px-2 gap-3"
              onClick={() => {
                onAddNode(shape.type);
                setActiveTool(shape.type);
              }}
            >
              <shape.icon className="h-4 w-4" />
              <span className="text-sm font-medium">{shape.label}</span>
            </Button>
          ))}
        </PopoverContent>
      </Popover>

      <ToolbarButton 
        icon={Type} 
        label="Text (T)" 
        isActive={activeTool === 'text'} 
        onClick={() => {
            onAddNode('Text');
            setActiveTool('text');
        }} 
      />

      <ToolbarButton 
        icon={StickyNote} 
        label="Sticky Note (S)" 
        isActive={activeTool === 'sticky'} 
        onClick={() => {
            onAddNode('StickyNote');
            setActiveTool('sticky');
        }} 
      />

      <ToolbarButton 
        icon={ArrowUpRight} 
        label="Connector (C)" 
        isActive={activeTool === 'connector'} 
        onClick={() => setActiveTool('connector')} 
      />
    </div>
  );
}
