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
  Smartphone,
  Triangle,
  Hexagon,
  Grid,
  Play,
  FormInput,
  CreditCard
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
  { name: 'User', iconName: 'User', icon: User },
  { name: 'Settings', iconName: 'Settings', icon: Settings },
  { name: 'Search', iconName: 'Search', icon: Search },
  { name: 'Bell', iconName: 'Bell', icon: Bell },
  { name: 'Mail', iconName: 'Mail', icon: Mail },
  { name: 'Calendar', iconName: 'Calendar', icon: Calendar },
  { name: 'Camera', iconName: 'Camera', icon: Camera },
  { name: 'Heart', iconName: 'Heart', icon: Heart },
  { name: 'Star', iconName: 'Star', icon: Star },
  { name: 'Home', iconName: 'Home', icon: Home },
  { name: 'Map', iconName: 'Map', icon: Map },
  { name: 'Link', iconName: 'Link', icon: Link },
  { name: 'Lock', iconName: 'Lock', icon: Lock },
  { name: 'Globe', iconName: 'Globe', icon: Globe },
  { name: 'Cart', iconName: 'ShoppingCart', icon: ShoppingCart },
  { name: 'Zap', iconName: 'Zap', icon: Zap },
  { name: 'Check', iconName: 'CheckCircle2', icon: CheckCircle2 },
  { name: 'Alert', iconName: 'AlertCircle', icon: AlertCircle },
  { name: 'Help', iconName: 'HelpCircle', icon: HelpCircle },
  { name: 'File', iconName: 'FileText', icon: FileText },
  { name: 'Image', iconName: 'Image', icon: Image },
  { name: 'Video', iconName: 'Video', icon: Video },
  { name: 'Music', iconName: 'Music', icon: Music },
  { name: 'Share', iconName: 'Share2', icon: Share2 },
  { name: 'Download', iconName: 'Download', icon: Download },
  { name: 'Trash', iconName: 'Trash2', icon: Trash2 },
  { name: 'Filter', iconName: 'Filter', icon: Filter },
  { name: 'Refresh', iconName: 'RefreshCw', icon: RefreshCw },
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

  const general = [
    { type: 'rectangle', icon: Square, label: 'Rectangle' },
    { type: 'rectangle', icon: Square, label: 'Rounded', data: { color: 'rounded-[30px]' } },
    { type: 'circle', icon: Circle, label: 'Circle' },
    { type: 'circle', icon: Circle, label: 'Ellipse', data: { width: 180, height: 100 } },
    { type: 'diamond', icon: Diamond, label: 'Diamond' },
    { type: 'triangle', icon: Triangle, label: 'Triangle' },
    { type: 'hexagon', icon: Hexagon, label: 'Hexagon' },
    { type: 'parallelogram', icon: Grid, label: 'Parallelogram' },
    { type: 'text', icon: Type, label: 'Text' },
  ];

  const flowchart = [
    { type: 'rectangle', icon: Square, label: 'Process' },
    { type: 'diamond', icon: Diamond, label: 'Decision' },
    { type: 'rectangle', icon: FileText, label: 'Document', data: { color: 'border-b-8 border-b-primary/50' } },
    { type: 'database', icon: Database, label: 'Data' },
    { type: 'circle', icon: Circle, label: 'Start / End', data: { width: 80, height: 80, color: 'border-4' } },
    { type: 'cloud', icon: Cloud, label: 'Cloud' },
    { type: 'parallelogram', icon: Grid, label: 'Input/Output', data: { label: 'Input/Output' } },
    { type: 'hexagon', icon: Hexagon, label: 'Preparation', data: { label: 'Preparation' } },
    { type: 'rectangle', icon: Square, label: 'Subprocess', data: { color: 'border-double border-4' } },
  ];

  const entityRelation = [
    { type: 'rectangle', icon: Square, label: 'Entity', data: { width: 200, height: 60, color: 'bg-primary/5 font-bold' } },
    { type: 'circle', icon: Circle, label: 'Attribute', data: { width: 140, height: 80, color: 'border-dashed' } },
    { type: 'diamond', icon: Diamond, label: 'Relationship', data: { color: 'bg-muted' } },
  ];

  const infrastructure = [
    { type: 'infrastructure', label: 'AWS', icon: Cloud, iconName: 'Cloud', data: { color: 'bg-orange-500/10 text-orange-600 border-orange-500/50', sublabel: 'Cloud Service' } },
    { type: 'infrastructure', label: 'Kubernetes', icon: Box, iconName: 'Box', data: { color: 'bg-blue-500/10 text-blue-600 border-blue-500/50', sublabel: 'Cluster' } },
    { type: 'infrastructure', label: 'Docker', icon: Container, iconName: 'Container', data: { color: 'bg-cyan-500/10 text-cyan-600 border-cyan-500/50', sublabel: 'Container' } },
    { type: 'infrastructure', label: 'Server', icon: Server, iconName: 'Server', data: { color: 'bg-slate-500/10 text-slate-600 border-slate-500/50', sublabel: 'Compute' } },
    { type: 'infrastructure', label: 'Load Balancer', icon: Network, iconName: 'Network', data: { color: 'bg-indigo-500/10 text-indigo-600 border-indigo-500/50', sublabel: 'Network' } },
    { type: 'infrastructure', label: 'Firewall', icon: Shield, iconName: 'Shield', data: { color: 'bg-red-500/10 text-red-600 border-red-500/50', sublabel: 'Security' } },
    { type: 'infrastructure', label: 'API Gateway', icon: Zap, iconName: 'Zap', data: { color: 'bg-yellow-500/10 text-yellow-600 border-yellow-500/50', sublabel: 'API' } },
    { type: 'infrastructure', label: 'Database', icon: Database, iconName: 'DatabaseIcon', data: { color: 'bg-emerald-500/10 text-emerald-600 border-emerald-500/50', sublabel: 'Storage' } },
  ];

  const devices = [
    { type: 'client', icon: User, label: 'User' },
    { type: 'mobile', icon: Smartphone, label: 'Mobile App' },
    { type: 'browser', icon: Monitor, label: 'Web App' },
  ];

  const wireframes = [
    { type: 'stickynote', icon: StickyNote, label: 'Sticky Note' },
    { type: 'browser', icon: Monitor, label: 'Browser Window' },
    { type: 'rectangle', icon: Play, label: 'Button UI', data: { width: 120, height: 40, color: 'bg-primary text-primary-foreground border-primary rounded-md font-semibold text-center flex items-center justify-center text-xs' } },
    { type: 'rectangle', icon: FormInput, label: 'Input UI', data: { width: 180, height: 40, color: 'bg-background border-input text-muted-foreground rounded-md text-left px-3 text-xs flex items-center' } },
    { type: 'rectangle', icon: CreditCard, label: 'Card UI', data: { width: 220, height: 160, color: 'bg-card border shadow-sm rounded-lg flex items-center justify-center' } },
  ];

  const filteredIcons = ICON_LIST.filter(i => i.name.toLowerCase().includes(iconSearch.toLowerCase()));

  const [expandedSection, setExpandedSection] = useState<string | null>('basic');

  const toggleSection = (section: string) => {
    setExpandedSection(prev => prev === section ? null : section);
  };

  return (
    <motion.div 
      initial={{ x: -20, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      className={cn("flex flex-col bg-background/95 backdrop-blur-xl border-r shadow-2xl h-full w-72 flex-shrink-0 z-20 pointer-events-auto", className)}
    >
      {/* Top action bar */}
      <div className="flex items-center gap-1 p-3 border-b bg-muted/20">
        <ToolbarButton 
          icon={MousePointer2} 
          label="Select (V)" 
          isActive={activeTool === 'select'} 
          onClick={() => setActiveTool('select')} 
          className="flex-1 h-9"
        />
        <ToolbarButton 
          icon={Type} 
          label="Text (T)" 
          isActive={activeTool === 'text'} 
          onClick={() => {
              onAddNode('Text');
              setActiveTool('text');
          }} 
          className="flex-1 h-9"
        />
        <ToolbarButton 
          icon={ArrowUpRight} 
          label="Connector (C)" 
          isActive={activeTool === 'connector'} 
          onClick={() => setActiveTool('connector')} 
          className="flex-1 h-9"
        />
      </div>

      <ScrollArea className="flex-1">
        <div className="flex flex-col">
          {/* General Shapes Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('basic')}
            >
              General
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'basic' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'basic' && (
              <div className="grid grid-cols-3 gap-2 p-3 pt-0 bg-muted/10">
                {general.map((shape, i) => (
                  <Button
                    key={`${shape.type}-${i}`}
                    variant="outline"
                    className="flex flex-col h-16 gap-1 p-2 bg-background hover:bg-primary/5 hover:border-primary/50 transition-all shadow-sm"
                    onClick={() => {
                      onAddNode(shape.type, shape.data);
                      setActiveTool(shape.type);
                    }}
                    title={shape.label}
                  >
                    <shape.icon className="h-5 w-5 text-foreground" />
                    <span className="text-[9px] font-semibold tracking-tighter truncate w-full">{shape.label}</span>
                  </Button>
                ))}
              </div>
            )}
          </div>

          {/* Flowchart Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('flowchart')}
            >
              Flowchart
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'flowchart' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'flowchart' && (
              <div className="grid grid-cols-3 gap-2 p-3 pt-0 bg-muted/10">
                {flowchart.map((shape, i) => (
                  <Button
                    key={`${shape.type}-${i}`}
                    variant="outline"
                    className="flex flex-col h-16 gap-1 p-2 bg-background hover:bg-primary/5 hover:border-primary/50 transition-all shadow-sm"
                    onClick={() => {
                      onAddNode(shape.type, shape.data);
                      setActiveTool(shape.type);
                    }}
                    title={shape.label}
                  >
                    <shape.icon className="h-5 w-5 text-foreground" />
                    <span className="text-[9px] font-semibold tracking-tighter truncate w-full">{shape.label}</span>
                  </Button>
                ))}
              </div>
            )}
          </div>

          {/* Entity Relation Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('er')}
            >
              Entity Relation
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'er' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'er' && (
              <div className="grid grid-cols-3 gap-2 p-3 pt-0 bg-muted/10">
                {entityRelation.map((shape, i) => (
                  <Button
                    key={`${shape.type}-${i}`}
                    variant="outline"
                    className="flex flex-col h-16 gap-1 p-2 bg-background hover:bg-primary/5 hover:border-primary/50 transition-all shadow-sm"
                    onClick={() => {
                      onAddNode(shape.type, shape.data);
                      setActiveTool(shape.type);
                    }}
                    title={shape.label}
                  >
                    <shape.icon className="h-5 w-5 text-foreground" />
                    <span className="text-[9px] font-semibold tracking-tighter truncate w-full">{shape.label}</span>
                  </Button>
                ))}
              </div>
            )}
          </div>

          {/* Wireframes Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('wireframes')}
            >
              Wireframes
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'wireframes' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'wireframes' && (
              <div className="grid grid-cols-2 gap-2 p-3 pt-0 bg-muted/10">
                {wireframes.map((wf) => (
                  <Button
                    key={wf.label}
                    variant="outline"
                    className="flex items-center justify-start h-12 gap-3 px-3 bg-background hover:bg-primary/5 hover:border-primary/50 transition-all shadow-sm"
                    onClick={() => {
                      onAddNode(wf.type, wf.data);
                      setActiveTool(wf.type);
                    }}
                  >
                    <wf.icon className="h-5 w-5 text-foreground" />
                    <span className="text-xs font-semibold">{wf.label}</span>
                  </Button>
                ))}
              </div>
            )}
          </div>

          {/* Devices Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('devices')}
            >
              Devices & Users
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'devices' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'devices' && (
              <div className="grid grid-cols-2 gap-2 p-3 pt-0 bg-muted/10">
                {devices.map((device) => (
                  <Button
                    key={device.label}
                    variant="outline"
                    className="flex flex-col h-16 gap-1 p-2 bg-background hover:bg-primary/5 hover:border-primary/50 transition-all shadow-sm"
                    onClick={() => {
                        onAddNode(device.type);
                        setActiveTool(device.type);
                    }}
                  >
                    <device.icon className="h-5 w-5 text-foreground" />
                    <span className="text-[10px] font-bold text-center leading-none">{device.label}</span>
                  </Button>
                ))}
              </div>
            )}
          </div>

          {/* Infrastructure Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('infra')}
            >
              Infrastructure
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'infra' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'infra' && (
              <div className="grid grid-cols-2 gap-2 p-3 pt-0 bg-muted/10">
                {infrastructure.map((infra) => (
                    <Button
                    key={infra.label}
                    variant="outline"
                    className="flex flex-col h-16 gap-1.5 p-2 bg-background hover:bg-primary/5 hover:border-primary/50 transition-all shadow-sm group"
                    onClick={() => {
                        onAddNode(infra.type, { ...infra.data, iconName: infra.iconName, label: infra.label });
                        setActiveTool(infra.type);
                    }}
                    >
                    <infra.icon className={cn("h-6 w-6 transition-transform group-hover:scale-110", infra.data.color.split(' ')[1])} />
                    <span className="text-[10px] font-bold text-center leading-none text-muted-foreground group-hover:text-foreground">{infra.label}</span>
                    </Button>
                ))}
              </div>
            )}
          </div>

          {/* Icons Accordion */}
          <div className="border-b">
            <button 
              className="flex items-center justify-between w-full p-3 hover:bg-muted/50 transition-colors text-sm font-bold uppercase tracking-wider text-muted-foreground"
              onClick={() => toggleSection('icons')}
            >
              Icon Library
              <Plus className={cn("h-4 w-4 transition-transform", expandedSection === 'icons' ? "rotate-45" : "")} />
            </button>
            {expandedSection === 'icons' && (
              <div className="p-3 pt-0 bg-muted/10 space-y-3">
                <div className="relative">
                  <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                  <Input 
                    placeholder="Search icons..." 
                    className="h-9 pl-8 text-xs bg-background" 
                    value={iconSearch}
                    onChange={(e) => setIconSearch(e.target.value)}
                  />
                </div>
                <div className="grid grid-cols-5 gap-1">
                  {filteredIcons.map((item) => (
                    <Button
                      key={item.name}
                      variant="ghost"
                      size="icon"
                      className="h-10 w-10 bg-background border hover:border-primary/50 hover:bg-primary/5 shadow-sm"
                      onClick={() => {
                        onAddNode('Icon', { iconName: item.iconName, label: '' });
                        setActiveTool('icon');
                      }}
                      title={item.name}
                    >
                      <item.icon className="h-4 w-4" />
                    </Button>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
      </ScrollArea>
    </motion.div>
  );
}
