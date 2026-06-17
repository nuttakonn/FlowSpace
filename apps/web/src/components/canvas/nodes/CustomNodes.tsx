"use client";

import React, { memo, useState, useEffect } from 'react';
import { Handle, Position, NodeProps } from '@xyflow/react';
import { Layers } from 'lucide-react';
import { useCanvasStore } from '@/store/useCanvasStore';
import { cn } from '@/lib/utils';

const handleStyle = { width: 8, height: 8, background: '#3b82f6', border: '2px solid white' };

interface CustomNodeData {
  label?: string;
}

const NodeLabelInput = ({ id, label }: { id: string; label?: string }) => {
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
      className="nodrag nowheel w-full bg-transparent text-center focus:outline-none resize-none overflow-hidden text-xs font-medium"
      rows={1}
      spellCheck={false}
      style={{ minHeight: '1.2em' }}
    />
  );
};

export const RectangleNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  return (
    <div className={`flex h-20 w-40 items-center justify-center rounded-lg border-2 border-primary bg-background px-4 py-2 text-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
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
  return (
    <div className={`relative flex items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <div 
        className="w-32 h-32 bg-background border-2 border-primary rotate-45 flex items-center justify-center"
        style={{ clipPath: 'polygon(50% 0%, 100% 50%, 50% 100%, 0% 50%)' }}
      >
        <div className="-rotate-45 text-center px-4 w-full">
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
  return (
    <div className={`flex h-24 w-24 items-center justify-center rounded-full border-2 border-primary bg-background p-4 text-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <NodeLabelInput id={id} label={nodeData.label} />
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const DatabaseNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  return (
    <div className={`relative flex h-24 w-24 flex-col items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <div className="absolute top-0 h-4 w-full rounded-[50%] border-2 border-primary bg-background z-10" />
      <div className="flex h-full w-full flex-col items-center justify-center border-x-2 border-b-2 border-primary bg-background rounded-b-lg pt-4 px-2">
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
  return (
    <div className={`relative flex h-20 w-36 items-center justify-center transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <div className="absolute inset-0 bg-background border-2 border-primary rounded-[40%] flex items-center justify-center overflow-hidden">
        <div className="absolute -top-4 -left-2 h-12 w-12 rounded-full border-2 border-primary bg-background" />
        <div className="absolute -top-6 left-8 h-14 w-14 rounded-full border-2 border-primary bg-background" />
        <div className="absolute -top-4 right-2 h-10 w-10 rounded-full border-2 border-primary bg-background" />
      </div>
      <div className="z-10 text-center px-6 w-full">
        <NodeLabelInput id={id} label={nodeData.label} />
      </div>
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const BrowserNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  return (
    <div className={`flex flex-col h-64 w-96 rounded-lg border-2 border-primary bg-background shadow-lg overflow-hidden transition-all ${selected ? 'ring-2 ring-primary ring-offset-2' : ''}`}>
      <div className="h-6 border-b-2 border-primary bg-muted flex items-center px-2 gap-1">
        <div className="h-2 w-2 rounded-full bg-red-400" />
        <div className="h-2 w-2 rounded-full bg-yellow-400" />
        <div className="h-2 w-2 rounded-full bg-green-400" />
        <div className="ml-2 h-3 w-32 rounded bg-background" />
      </div>
      <div className="flex-1 p-6 flex items-center justify-center text-center">
        <div className="w-full">
          <NodeLabelInput id={id} label={nodeData.label} />
        </div>
      </div>
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const StickyNoteNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  return (
    <div className={`flex h-40 w-40 flex-col items-center justify-center bg-yellow-100 shadow-md p-6 text-center transition-all ${selected ? 'ring-2 ring-yellow-400 ring-offset-2' : ''}`}>
      <NodeLabelInput id={id} label={nodeData.label} />
      <div className="absolute bottom-0 right-0 h-6 w-6 bg-yellow-200" style={{ clipPath: 'polygon(100% 0, 0 100%, 100% 100%)' }} />
      <Handle type="target" position={Position.Top} style={handleStyle} />
      <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

export const TextNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as CustomNodeData;
  return (
    <div className={`p-2 min-w-[100px] transition-all ${selected ? 'ring-2 ring-primary ring-offset-2 rounded' : ''}`}>
      <NodeLabelInput id={id} label={nodeData.label} />
      <Handle type="target" position={Position.Top} style={{ visibility: 'hidden' }} />
      <Handle type="source" position={Position.Bottom} style={{ visibility: 'hidden' }} />
    </div>
  );
});

export const InfrastructureNode = memo(({ id, data, selected }: NodeProps) => {
  const nodeData = data as any;
  const Icon = nodeData.iconComponent;
  return (
    <div className={`flex flex-col items-center justify-center p-4 rounded-xl border-2 bg-background transition-all ${selected ? 'border-primary ring-2 ring-primary/20' : 'border-border'}`}>
       <div className={cn("p-3 rounded-lg mb-2", nodeData.color || "bg-primary/10 text-primary")}>
          {Icon ? <Icon className="h-8 w-8" /> : <Layers className="h-8 w-8" />}
       </div>
       <div className="text-center min-w-[80px]">
          <NodeLabelInput id={id} label={nodeData.label} />
       </div>
       <Handle type="target" position={Position.Top} style={handleStyle} />
       <Handle type="source" position={Position.Bottom} style={handleStyle} />
       <Handle type="source" position={Position.Left} style={handleStyle} />
       <Handle type="source" position={Position.Right} style={handleStyle} />
    </div>
  );
});

export const IconNode = memo(({ data, selected }: NodeProps) => {
  const nodeData = data as any;
  const Icon = nodeData.iconComponent;
  return (
    <div className={`flex items-center justify-center p-2 rounded-lg border-2 border-transparent transition-all ${selected ? 'border-primary bg-primary/5' : ''}`}>
       {Icon ? <Icon className="h-10 w-10 text-primary" /> : <Layers className="h-10 w-10" />}
       <Handle type="target" position={Position.Top} style={handleStyle} />
       <Handle type="source" position={Position.Bottom} style={handleStyle} />
    </div>
  );
});

DiamondNode.displayName = 'DiamondNode';
CircleNode.displayName = 'CircleNode';
DatabaseNode.displayName = 'DatabaseNode';
CloudNode.displayName = 'CloudNode';
BrowserNode.displayName = 'BrowserNode';
StickyNoteNode.displayName = 'StickyNoteNode';
IconNode.displayName = 'IconNode';
RectangleNode.displayName = 'RectangleNode';
TextNode.displayName = 'TextNode';
InfrastructureNode.displayName = 'InfrastructureNode';
