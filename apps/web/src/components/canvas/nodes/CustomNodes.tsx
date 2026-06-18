"use client";

import React, { memo, useState, useEffect } from 'react';
import { Handle, Position, NodeProps, NodeResizer } from '@xyflow/react';
import { 
  Layers, 
  Monitor, 
  Smartphone, 
  Shield, 
  Wifi, 
  Cpu, 
  Cloud, 
  Box, 
  Server,
  Zap,
  Globe,
  User,
  Database as DatabaseIcon,
  ChevronRightSquare,
  Activity,
  Terminal,
  Container,
  GitBranch,
  Search,
  Lock,
  MessageSquare,
  FileCode,
  HardDrive
} from 'lucide-react';
import * as LucideIcons from 'lucide-react';
import { useCanvasStore } from '@/store/useCanvasStore';
import { cn } from '@/lib/utils';
import { SelectionToolbar } from '../SelectionToolbar';

const handleStyle = { width: 8, height: 8, background: '#3b82f6', border: '2px solid white' };

interface CustomNodeData {
  label?: string;
  iconName?: string;
  color?: string;
  sublabel?: string;
  width?: number;
  height?: number;
}

const NodeLabelInput = ({ id, label, className }: { id: string; label?: string; className?: string }) => {
  const [value, setValue] = useState(label || '');
  const updateNodeLabel = useCanvasStore(s => s.updateNodeLabel);

  useEffect(() => {
    setValue(label || '');
  }, [label]);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setValue(e.target.value);
  };

  const handleBlur = () => {
    updateNodeLabel(id, value);
  };

  return (
    <textarea
      value={value}
      onChange={handleChange}
      onBlur={handleBlur}
      className={cn(
        "nodrag nowheel w-full bg-transparent text-center focus:outline-none resize-none overflow-hidden text-xs font-medium leading-relaxed whitespace-pre-wrap break-words",
        className
      )}
      rows={1}
      spellCheck={false}
      style={{ minHeight: '1.2em' }}
    />
  );
};

export const RectangleNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 180, 
    height: nodeData.height || 100 
  };
  return (
    <div style={style} className={`group flex items-center justify-center rounded-xl border-2 border-primary bg-background px-4 py-2 text-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <NodeResizer minWidth={80} minHeight={40} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <NodeLabelInput id={id} label={nodeData.label} />
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
      <Handle type="source" position={Position.Left} style={handleStyle} />
      <Handle type="source" position={Position.Right} style={handleStyle} />
    </div>
  );
});

export const DiamondNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 150, 
    height: nodeData.height || 150 
  };
  return (
    <div style={style} className={`group relative flex items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <NodeResizer minWidth={100} minHeight={100} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <div 
        className="w-full h-full bg-background border-2 border-primary rotate-45 flex items-center justify-center"
        style={{ clipPath: 'polygon(50% 0%, 100% 50%, 50% 100%, 0% 50%)' }}
      >
        <div className="-rotate-45 text-center px-6 w-full flex items-center justify-center">
          <NodeLabelInput id={id} label={nodeData.label} />
        </div>
      </div>
      
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
      <Handle type="source" position={Position.Left} style={handleStyle} />
      <Handle type="source" position={Position.Right} style={handleStyle} />
    </div>
  );
});

export const CircleNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 120, 
    height: nodeData.height || 120 
  };
  return (
    <div style={style} className={`group flex items-center justify-center rounded-full border-2 border-primary bg-background p-4 text-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <NodeResizer minWidth={60} minHeight={60} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <NodeLabelInput id={id} label={nodeData.label} />
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const DatabaseNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 100, 
    height: nodeData.height || 120 
  };
  return (
    <div style={style} className={`group relative flex flex-col items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <NodeResizer minWidth={60} minHeight={80} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <div className="absolute top-0 h-[15%] w-full rounded-[50%] border-2 border-primary bg-background z-10" />
      <div className="flex h-full w-full flex-col items-center justify-center border-x-2 border-b-2 border-primary bg-background rounded-b-lg pt-[15%] px-2">
        <p className="text-[8px] font-bold uppercase text-muted-foreground mb-1">DB</p>
        <NodeLabelInput id={id} label={nodeData.label} />
      </div>
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const CloudNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 200, 
    height: nodeData.height || 120 
  };
  return (
    <div style={style} className={`group relative flex items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <NodeResizer minWidth={120} minHeight={80} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <div className="absolute inset-0 bg-background border-2 border-primary rounded-[40%] flex items-center justify-center overflow-hidden">
        <div className="absolute -top-4 -left-2 h-[60%] w-[40%] rounded-full border-2 border-primary bg-background" />
        <div className="absolute -top-6 left-[30%] h-[70%] w-[50%] rounded-full border-2 border-primary bg-background" />
        <div className="absolute -top-4 right-2 h-[50%] w-[35%] rounded-full border-2 border-primary bg-background" />
      </div>
      <div className="z-10 text-center px-6 w-full">
        <NodeLabelInput id={id} label={nodeData.label} />
      </div>
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const InfrastructureNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const iconName = nodeData.iconName as keyof typeof LucideIcons | undefined;
  const Icon = iconName ? LucideIcons[iconName] as React.ElementType : null;
  const style = { 
    width: nodeData.width || 140, 
    height: nodeData.height || 140 
  };
  return (
    <div style={style} className={`group flex flex-col items-center justify-center p-4 rounded-2xl border-2 bg-background transition-all ${selected ? 'border-primary ring-2 ring-primary/20 shadow-xl scale-[1.02]' : 'border-border shadow-sm'}`}>
       <NodeResizer minWidth={120} minHeight={120} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
       <SelectionToolbar isVisible={selected} nodeId={id} />
       <div className={cn("p-4 rounded-xl mb-3 shadow-inner transition-colors", nodeData.color || "bg-primary/10 text-primary")}>
          {Icon ? <Icon className="h-8 w-8" /> : <LucideIcons.Layers className="h-8 w-8" />}
       </div>
       <div className="text-center w-full px-2">
          <NodeLabelInput id={id} label={nodeData.label} className="text-sm font-bold" />
          {nodeData.sublabel && <p className="text-[10px] text-muted-foreground mt-0.5 uppercase tracking-tighter">{nodeData.sublabel}</p>}
       </div>
       <Handle type="target" position={Position.Top} style={handleStyle} />
       <Handle type="source" position={Position.Bottom} style={handleStyle} />
       <Handle type="source" position={Position.Left} style={handleStyle} />
       <Handle type="source" position={Position.Right} style={handleStyle} />
    </div>
  );
});

export const ClientNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 120, 
    height: nodeData.height || 120 
  };
  return (
    <div style={style} className={`group flex flex-col items-center justify-center p-4 transition-all ${selected ? 'ring-2 ring-primary ring-offset-2 rounded-xl bg-primary/5' : ''}`}>
       <NodeResizer minWidth={100} minHeight={100} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
       <SelectionToolbar isVisible={selected} nodeId={id} />
       <div className="p-4 rounded-full bg-primary/10 text-primary mb-2">
          <LucideIcons.User className="h-8 w-8" />
       </div>
       <NodeLabelInput id={id} label={nodeData.label} className="text-sm font-bold" />
       <Handle type="target" position={Position.Top} style={handleStyle} />
       <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const MobileNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 120, 
    height: nodeData.height || 200 
  };
  return (
    <div style={style} className={`group flex flex-col items-center justify-center p-4 rounded-[2rem] border-[3px] border-primary bg-background shadow-xl transition-all ${selected ? 'ring-4 ring-primary/20' : ''}`}>
       <NodeResizer minWidth={80} minHeight={140} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
       <SelectionToolbar isVisible={selected} nodeId={id} />
       <div className="w-12 h-1 bg-primary/20 rounded-full mb-6 mt-1" />
       <div className="flex-1 flex items-center justify-center w-full">
          <NodeLabelInput id={id} label={nodeData.label} className="text-xs font-bold" />
       </div>
       <div className="w-4 h-4 border-2 border-primary/40 rounded-full mt-4 mb-1" />
       <Handle type="target" position={Position.Top} style={handleStyle} />
       <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const BrowserNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 400, 
    height: nodeData.height || 280 
  };
  return (
    <div style={style} className={`group flex flex-col rounded-xl border-2 border-primary bg-background shadow-2xl overflow-hidden transition-all ${selected ? 'ring-4 ring-primary/20' : ''}`}>
      <NodeResizer minWidth={300} minHeight={200} isVisible={selected} lineClassName="border-primary" handleClassName="h-4 w-4 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <div className="h-8 border-b-2 border-primary bg-muted flex items-center px-3 gap-1.5 flex-shrink-0">
        <div className="h-3 w-3 rounded-full bg-red-400" />
        <div className="h-3 w-3 rounded-full bg-yellow-400" />
        <div className="h-3 w-3 rounded-full bg-green-400" />
        <div className="ml-3 h-4 w-48 rounded-md bg-background border flex items-center px-2">
            <div className="h-1.5 w-1.5 rounded-full bg-muted-foreground/30 mr-2" />
            <div className="h-1 w-24 bg-muted-foreground/10 rounded-full" />
        </div>
      </div>
      <div className="flex-1 p-8 flex items-center justify-center text-center overflow-auto">
        <div className="w-full max-w-md">
          <NodeLabelInput id={id} label={nodeData.label} className="text-lg font-bold tracking-tight" />
          <div className="h-1 w-12 bg-primary/20 mx-auto mt-4 rounded-full" />
        </div>
      </div>
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const StickyNoteNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 180, 
    height: nodeData.height || 180 
  };
  return (
    <div style={style} className={`group relative flex flex-col items-center justify-center bg-yellow-50 border-2 border-yellow-200/50 shadow-xl p-8 text-center transition-all ${selected ? 'ring-4 ring-yellow-400/30' : ''}`}>
      <NodeResizer minWidth={150} minHeight={150} isVisible={selected} lineClassName="border-yellow-400" handleClassName="h-3 w-3 bg-white border-2 border-yellow-400 rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <NodeLabelInput id={id} label={nodeData.label} className="text-base font-semibold text-yellow-900" />
      <div className="absolute bottom-0 right-0 h-10 w-10 bg-yellow-100/80" style={{ clipPath: 'polygon(100% 0, 0 100%, 100% 100%)' }} />
      <Handle type="target" position={Position.Top} style={{ ...handleStyle, background: '#ca8a04' }} />
      <Handle type="source" position={Position.Bottom} style={{ ...handleStyle, background: '#ca8a04' }} />
    </div>
  );
});

export const TextNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const style = { 
    width: nodeData.width || 120, 
    height: nodeData.height || 50 
  };
  return (
    <div style={style} className={`group p-4 flex items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2 rounded-lg bg-primary/5' : ''}`}>
      <NodeResizer minWidth={100} minHeight={40} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
      <SelectionToolbar isVisible={selected} nodeId={id} />
      <NodeLabelInput id={id} label={nodeData.label} className="text-sm font-medium" />
      <Handle type="target" position={Position.Top} style={{ opacity: 0 }} />
      <Handle type="source" position={Position.Bottom} style={{ opacity: 0 }} />
    </div>
  );
});

export const IconNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  const iconName = nodeData.iconName as keyof typeof LucideIcons | undefined;
  const Icon = iconName ? LucideIcons[iconName] as React.ElementType : null;
  const style = { 
    width: nodeData.width || 80, 
    height: nodeData.height || 80 
  };
  return (
    <div style={style} className={`group flex items-center justify-center p-4 rounded-xl border-2 border-transparent transition-all ${selected ? 'border-primary bg-primary/5 shadow-lg' : ''}`}>
       <NodeResizer minWidth={60} minHeight={60} isVisible={selected} lineClassName="border-primary" handleClassName="h-3 w-3 bg-white border-2 border-primary rounded-full" />
       <SelectionToolbar isVisible={selected} nodeId={id} />
       {Icon ? <Icon className="h-full w-full text-primary" /> : <LucideIcons.Layers className="h-full w-full" />}
       <Handle type="target" position={Position.Top} style={handleStyle} />
       <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

RectangleNode.displayName = 'RectangleNode';
DiamondNode.displayName = 'DiamondNode';
CircleNode.displayName = 'CircleNode';
DatabaseNode.displayName = 'DatabaseNode';
CloudNode.displayName = 'CloudNode';
BrowserNode.displayName = 'BrowserNode';
StickyNoteNode.displayName = 'StickyNoteNode';
IconNode.displayName = 'IconNode';
TextNode.displayName = 'TextNode';
InfrastructureNode.displayName = 'InfrastructureNode';
ClientNode.displayName = 'ClientNode';
MobileNode.displayName = 'MobileNode';
