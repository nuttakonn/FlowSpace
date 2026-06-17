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
  LucideIcon,
  Monitor,
  Search,
  Settings,
  User,
  Bell,
  Mail,
  Calendar,
  Camera,
  Heart,
  Star,
  Home,
  Map,
  Link,
  Lock,
  Globe,
  ShoppingCart,
  Zap,
  CheckCircle2,
  AlertCircle,
  HelpCircle,
  FileText,
  Image,
  Video,
  Music,
  Share2,
  Download,
  Trash2,
  Filter,
  RefreshCw,
  MoreHorizontal,
  Box,
  Server,
  Shield,
  Activity,
  Cpu,
  Container,
  HardDrive,
  Network,
  Smartphone
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ScrollArea } from '@/components/ui/scroll-area';
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

const ICON_LIST = [
  { name: 'User', icon: User },
  { name: 'Settings', icon: Settings },
  { name: 'Search', icon: Search },
  { name: 'Bell', icon: Bell },
  { name: 'Mail', icon: Mail },
  { name: 'Calendar', icon: Calendar },
  { name: 'Camera', icon: Camera },
  { name: 'Heart', icon: Heart },
  { name: 'Star', icon: Star },
  { name: 'Home', icon: Home },
  { name: 'Map', icon: Map },
  { name: 'Link', icon: Link },
  { name: 'Lock', icon: Lock },
  { name: 'Globe', icon: Globe },
  { name: 'Cart', icon: ShoppingCart },
  { name: 'Zap', icon: Zap },
  { name: 'Check', icon: CheckCircle2 },
  { name: 'Alert', icon: AlertCircle },
  { name: 'Help', icon: HelpCircle },
  { name: 'File', icon: FileText },
  { name: 'Image', icon: Image },
  { name: 'Video', icon: Video },
  { name: 'Music', icon: Music },
  { name: 'Share', icon: Share2 },
  { name: 'Download', icon: Download },
  { name: 'Trash', icon: Trash2 },
  { name: 'Filter', icon: Filter },
  { name: 'Refresh', icon: RefreshCw },
];

import { motion } from 'framer-motion';

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
        <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
          <Button
            variant={isActive ? "secondary" : "ghost"}
            size="icon"
            className={cn("h-10 w-10 rounded-lg", isActive && "bg-primary/10 text-primary", className)}
            onClick={onClick}
          >
            <Icon className="h-5 w-5" />
          </Button>
        </motion.div>
      </TooltipTrigger>
      <TooltipContent side="right">
        <p>{label}</p>
      </TooltipContent>
    </Tooltip>
  </TooltipProvider>
);

interface FloatingToolbarProps {
  onAddNode: (type: string, data?: any) => void;
  className?: string;
}

export function FloatingToolbar({ onAddNode, className }: FloatingToolbarProps) {
  const [activeTool, setActiveTool] = useState('select');
  const [iconSearch, setIconSearch] = useState('');

  const shapes = [
    { type: 'Rectangle', icon: Square, label: 'Rectangle' },
    { type: 'Circle', icon: Circle, label: 'Circle' },
    { type: 'Diamond', icon: Diamond, label: 'Decision' },
    { type: 'Database', icon: Database, label: 'Database' },
    { type: 'Cloud', icon: Cloud, label: 'General Cloud' },
  ];

  const infrastructure = [
    { type: 'Infrastructure', label: 'AWS', icon: Cloud, data: { color: 'bg-orange-500/10 text-orange-600', sublabel: 'Cloud Service' } },
    { type: 'Infrastructure', label: 'Kubernetes', icon: Box, data: { color: 'bg-blue-500/10 text-blue-600', sublabel: 'Cluster' } },
    { type: 'Infrastructure', label: 'Docker', icon: Container, data: { color: 'bg-cyan-500/10 text-cyan-600', sublabel: 'Container' } },
    { type: 'Infrastructure', label: 'Server', icon: Server, data: { color: 'bg-slate-500/10 text-slate-600', sublabel: 'Compute' } },
    { type: 'Infrastructure', label: 'Load Balancer', icon: Network, data: { color: 'bg-indigo-500/10 text-indigo-600', sublabel: 'Network' } },
    { type: 'Infrastructure', label: 'Firewall', icon: Shield, data: { color: 'bg-red-500/10 text-red-600', sublabel: 'Security' } },
    { type: 'Infrastructure', label: 'API Gateway', icon: Zap, data: { color: 'bg-yellow-500/10 text-yellow-600', sublabel: 'API' } },
    { type: 'Infrastructure', label: 'Function', icon: Cpu, data: { color: 'bg-pink-500/10 text-pink-600', sublabel: 'Lambda' } },
    { type: 'Infrastructure', label: 'Kafka', icon: Activity, data: { color: 'bg-purple-500/10 text-purple-600', sublabel: 'Streaming' } },
    { type: 'Infrastructure', label: 'Storage', icon: HardDrive, data: { color: 'bg-emerald-500/10 text-emerald-600', sublabel: 'S3/EBS' } },
  ];

  const devices = [
    { type: 'Client', icon: User, label: 'User' },
    { type: 'Mobile', icon: Smartphone, label: 'Mobile App' },
    { type: 'Browser', icon: Monitor, label: 'Web App' },
  ];

  const wireframes = [
    { type: 'StickyNote', icon: StickyNote, label: 'Sticky Note' },
  ];

  const filteredIcons = ICON_LIST.filter(i => i.name.toLowerCase().includes(iconSearch.toLowerCase()));

  return (
    <motion.div 
      initial={{ x: -20, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      className={cn("flex flex-col gap-2 p-2 bg-background/80 backdrop-blur border rounded-xl shadow-lg", className)}
    >
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
              label="Add Content" 
              isActive={['Rectangle', 'Circle', 'Diamond', 'Database', 'Cloud', 'Infrastructure', 'Browser', 'StickyNote'].includes(activeTool)}
            />
          </div>
        </PopoverTrigger>
        <PopoverContent side="right" align="start" className="w-64 p-0 flex flex-col overflow-hidden max-h-[80vh]">
          <ScrollArea className="flex-1">
            <div className="p-2 flex flex-col gap-1">
              <p className="text-[10px] font-bold text-muted-foreground px-2 py-1 uppercase tracking-wider">Basic Shapes</p>
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
              
              <p className="text-[10px] font-bold text-muted-foreground px-2 py-1 uppercase tracking-wider mt-3">Devices & Apps</p>
          <div className="grid grid-cols-2 gap-1 px-1">
            {devices.map((device) => (
                <Button
                key={device.label}
                variant="ghost"
                size="sm"
                className="flex flex-col h-16 gap-1 p-2"
                onClick={() => {
                    onAddNode(device.type);
                    setActiveTool(device.type);
                }}
                >
                <device.icon className="h-5 w-5" />
                <span className="text-[10px] font-medium text-center leading-none">{device.label}</span>
                </Button>
            ))}
          </div>

          <p className="text-[10px] font-bold text-muted-foreground px-2 py-1 uppercase tracking-wider mt-3">Infrastructure</p>
              <div className="grid grid-cols-2 gap-1 px-1">
                {infrastructure.map((infra) => (
                    <Button
                    key={infra.label}
                    variant="ghost"
                    size="sm"
                    className="flex flex-col h-16 gap-1 p-2"
                    onClick={() => {
                        onAddNode(infra.type, { ...infra.data, iconComponent: infra.icon, label: infra.label });
                        setActiveTool(infra.type);
                    }}
                    >
                    <infra.icon className={cn("h-5 w-5", infra.data.color.split(' ')[1])} />
                    <span className="text-[10px] font-medium text-center leading-none">{infra.label}</span>
                    </Button>
                ))}
              </div>

              <p className="text-[10px] font-bold text-muted-foreground px-2 py-1 uppercase tracking-wider mt-3">Wireframes</p>
              {wireframes.map((wf) => (
                <Button
                  key={wf.type}
                  variant="ghost"
                  className="justify-start h-9 px-2 gap-3"
                  onClick={() => {
                    onAddNode(wf.type);
                    setActiveTool(wf.type);
                  }}
                >
                  <wf.icon className="h-4 w-4" />
                  <span className="text-sm font-medium">{wf.label}</span>
                </Button>
              ))}
            </div>
          </ScrollArea>
        </PopoverContent>
      </Popover>

      <Popover>
        <PopoverTrigger asChild>
          <div className="relative">
            <ToolbarButton 
              icon={MoreHorizontal} 
              label="Icons Library" 
              isActive={activeTool === 'icon'}
            />
          </div>
        </PopoverTrigger>
        <PopoverContent side="right" align="start" className="w-64 p-0 flex flex-col overflow-hidden">
          <div className="p-2 border-b">
            <div className="relative">
              <Search className="absolute left-2 top-2.5 h-3 w-3 text-muted-foreground" />
              <Input 
                placeholder="Search icons..." 
                className="h-8 pl-7 text-xs" 
                value={iconSearch}
                onChange={(e) => setIconSearch(e.target.value)}
              />
            </div>
          </div>
          <ScrollArea className="h-64 p-2">
            <div className="grid grid-cols-4 gap-1">
              {filteredIcons.map((item) => (
                <Button
                  key={item.name}
                  variant="ghost"
                  size="icon"
                  className="h-12 w-12"
                  onClick={() => {
                    onAddNode('Icon', { iconComponent: item.icon, label: '' });
                    setActiveTool('icon');
                  }}
                  title={item.name}
                >
                  <item.icon className="h-5 w-5" />
                </Button>
              ))}
            </div>
          </ScrollArea>
        </PopoverContent>
      </Popover>

      <ToolbarButton 
        icon={Type} 
        label="Text Tool (T)" 
        isActive={activeTool === 'text'} 
        onClick={() => {
            onAddNode('Text');
            setActiveTool('text');
        }} 
      />

      <ToolbarButton 
        icon={ArrowUpRight} 
        label="Connector (C)" 
        isActive={activeTool === 'connector'} 
        onClick={() => setActiveTool('connector')} 
      />
    </motion.div>
  );
}
