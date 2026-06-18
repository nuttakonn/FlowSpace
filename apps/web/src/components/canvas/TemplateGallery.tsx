"use client";

import React, { useState } from 'react';
import { 
  Layout, 
  Workflow, 
  Database, 
  Users, 
  Lightbulb, 
  Layers, 
  Search,
  Check,
  Plus,
  Loader2
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { 
  Dialog, 
  DialogContent, 
  DialogDescription, 
  DialogFooter, 
  DialogHeader, 
  DialogTitle 
} from '@/components/ui/dialog';
import { cn } from '@/lib/utils';
import { ScrollArea } from '@/components/ui/scroll-area';
import { motion, AnimatePresence } from 'framer-motion';

interface Template {
  id: string;
  name: string;
  description: string;
  category: string;
  boardType: string;
  icon: any;
  previewColor: string;
}

const TEMPLATES: Template[] = [
  { id: 'blank-whiteboard', name: 'Blank Whiteboard', description: 'Start with a clean canvas for free-form ideas.', category: 'Whiteboarding', boardType: 'Whiteboard', icon: Layers, previewColor: 'bg-blue-500' },
  { id: 'blank-flowchart', name: 'Blank Flowchart', description: 'Create structured diagrams and processes.', category: 'Diagrams', boardType: 'Flowchart', icon: Workflow, previewColor: 'bg-green-500' },
  { id: 'simple-flowchart', name: 'Simple Flowchart', description: 'Standard process flow template.', category: 'Diagrams', boardType: 'Flowchart', icon: Workflow, previewColor: 'bg-green-400' },
  { id: 'system-architecture', name: 'System Architecture', description: 'Plan your cloud or software infrastructure.', category: 'Architecture', boardType: 'Flowchart', icon: Database, previewColor: 'bg-purple-500' },
  { id: 'brainstorming', name: 'Brainstorming Session', description: 'Collaborative space for team ideas.', category: 'Whiteboarding', boardType: 'Whiteboard', icon: Lightbulb, previewColor: 'bg-yellow-500' },
  { id: 'org-chart', name: 'Organization Chart', description: 'Map out team structure and reporting.', category: 'Management', boardType: 'Flowchart', icon: Users, previewColor: 'bg-red-500' },
];

const CATEGORIES = ['All', 'Whiteboarding', 'Diagrams', 'Architecture', 'Management'];

interface TemplateGalleryProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSelect: (template: Template, name: string) => Promise<void>;
  isSubmitting?: boolean;
}

export function TemplateGallery({ isOpen, onOpenChange, onSelect, isSubmitting }: TemplateGalleryProps) {
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [selectedTemplate, setSelectedTemplate] = useState<Template | null>(TEMPLATES[0]);
  const [boardName, setBoardName] = useState('');

  const filteredTemplates = TEMPLATES.filter(t => 
    (selectedCategory === 'All' || t.category === selectedCategory) &&
    (t.name.toLowerCase().includes(search.toLowerCase()) || t.description.toLowerCase().includes(search.toLowerCase()))
  );

  const handleConfirm = () => {
    if (selectedTemplate && boardName) {
      onSelect(selectedTemplate, boardName);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[1200px] w-[95vw] h-[90vh] p-0 overflow-hidden bg-background border-none shadow-2xl">
        <DialogTitle className="sr-only">Choose a template</DialogTitle>
        <DialogDescription className="sr-only">Select a template to kickstart your workflow.</DialogDescription>
        
        <div className="flex h-full w-full overflow-hidden">
          {/* Fixed Sidebar - No shrinking allowed */}
          <div style={{ width: '280px', minWidth: '280px' }} className="hidden lg:flex flex-col border-r bg-muted/10 h-full overflow-hidden">
            <div className="p-8 pb-4">
              <h2 className="text-2xl font-bold tracking-tight text-primary">FlowSpace</h2>
              <p className="text-[10px] text-muted-foreground uppercase font-black tracking-widest mt-1">Template Library</p>
            </div>
            
            <div className="p-6 pt-0">
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input 
                  placeholder="Search templates..." 
                  className="pl-10 h-11 bg-background border-2 rounded-xl focus-visible:ring-primary/20" 
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
            </div>

            <ScrollArea className="flex-1 px-4">
              <div className="space-y-1.5 py-4">
                <p className="px-3 text-[10px] font-bold uppercase tracking-widest text-muted-foreground mb-2">Categories</p>
                {CATEGORIES.map(cat => (
                  <Button
                    key={cat}
                    variant={selectedCategory === cat ? "secondary" : "ghost"}
                    className={cn(
                      "w-full justify-start h-11 px-3 gap-4 transition-all rounded-xl",
                      selectedCategory === cat ? "bg-primary text-primary-foreground hover:bg-primary/90 shadow-md" : "hover:bg-primary/5"
                    )}
                    onClick={() => setSelectedCategory(cat)}
                  >
                    {cat === 'All' && <Layout className="h-4 w-4" />}
                    {cat === 'Whiteboarding' && <Layers className="h-4 w-4" />}
                    {cat === 'Diagrams' && <Workflow className="h-4 w-4" />}
                    {cat === 'Architecture' && <Database className="h-4 w-4" />}
                    {cat === 'Management' && <Users className="h-4 w-4" />}
                    <span className="text-sm font-bold">{cat}</span>
                  </Button>
                ))}
              </div>
            </ScrollArea>

            <div className="p-6 border-t bg-muted/10">
              <Button variant="outline" className="w-full justify-center h-11 rounded-xl font-bold" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
            </div>
          </div>

          {/* Main Content Area - Fill remaining space */}
          <div className="flex-1 flex flex-col min-w-0 bg-background h-full overflow-hidden relative">
            <div className="p-8 lg:p-10 pb-6 border-b lg:border-none flex-shrink-0">
              <h1 className="text-3xl lg:text-4xl font-black tracking-tight text-foreground">Choose a template</h1>
              <p className="text-base text-muted-foreground mt-2">Start your next big project with a professional foundation.</p>
              
              {/* Mobile Category Search (visible only on small screens) */}
              <div className="mt-6 flex flex-col gap-4 lg:hidden">
                 <div className="relative">
                    <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                    <Input 
                    placeholder="Search..." 
                    className="pl-10 h-11 bg-muted/30" 
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
                <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-hide">
                    {CATEGORIES.map(cat => (
                        <Button
                            key={cat}
                            variant={selectedCategory === cat ? "secondary" : "outline"}
                            size="sm"
                            className={cn("rounded-full px-4 h-9", selectedCategory === cat ? "bg-primary text-primary-foreground" : "")}
                            onClick={() => setSelectedCategory(cat)}
                        >
                            {cat}
                        </Button>
                    ))}
                </div>
              </div>
            </div>

            <ScrollArea className="flex-1 px-8 lg:px-10 py-2">
              <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-8 pb-12">
                <AnimatePresence mode="popLayout">
                  {filteredTemplates.map((template) => (
                    <motion.div
                      key={template.id}
                      layout
                      initial={{ opacity: 0, scale: 0.95 }}
                      animate={{ opacity: 1, scale: 1 }}
                      exit={{ opacity: 0, scale: 0.95 }}
                      whileHover={{ y: -8 }}
                      className={cn(
                        "group relative flex flex-col rounded-[1.5rem] border-2 cursor-pointer transition-all duration-300 overflow-hidden",
                        selectedTemplate?.id === template.id ? "border-primary bg-primary/5 shadow-2xl ring-2 ring-primary/20" : "border-border/50 bg-card hover:border-primary/50 hover:shadow-xl"
                      )}
                      onClick={() => {
                          setSelectedTemplate(template);
                          setBoardName(prev => prev || template.name);
                      }}
                    >
                      <div className={cn("aspect-[16/10] w-full flex items-center justify-center relative overflow-hidden flex-shrink-0 transition-transform duration-500 group-hover:scale-105", template.previewColor)}>
                          <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity" />
                          <template.icon className="h-20 w-16 text-white drop-shadow-2xl opacity-90" />
                      </div>
                      <div className="p-6 flex-1 flex flex-col">
                          <h3 className="font-bold text-lg leading-tight mb-2 text-foreground group-hover:text-primary transition-colors">{template.name}</h3>
                          <p className="text-xs text-muted-foreground leading-relaxed line-clamp-2">{template.description}</p>
                      </div>
                      {selectedTemplate?.id === template.id && (
                          <div className="absolute top-4 right-4 h-8 w-8 bg-primary text-primary-foreground rounded-full flex items-center justify-center shadow-lg animate-in zoom-in-50 duration-300">
                              <Check className="h-4 w-4 stroke-[3px]" />
                          </div>
                      )}
                    </motion.div>
                  ))}
                </AnimatePresence>
              </div>
            </ScrollArea>

            {/* Sticky Action Footer */}
            <div className="p-8 lg:p-10 border-t bg-background sticky bottom-0 z-20 shadow-[0_-10px_30px_rgba(0,0,0,0.03)]">
               <div className="max-w-4xl mx-auto flex flex-col md:flex-row items-end gap-6">
                  <div className="flex-1 w-full space-y-3">
                    <label className="text-[11px] font-black text-primary uppercase tracking-[0.2em] ml-1">Board Name</label>
                    <Input 
                      placeholder="Enter a descriptive name..." 
                      value={boardName}
                      onChange={(e) => setBoardName(e.target.value)}
                      className="h-14 text-xl font-bold bg-muted/20 border-2 border-transparent focus-visible:border-primary/30 focus-visible:ring-0 rounded-2xl px-6 transition-all"
                      autoFocus
                    />
                  </div>
                  <div className="flex gap-4 w-full md:w-auto">
                    <Button 
                      size="lg"
                      className="h-14 px-10 rounded-2xl font-black text-lg transition-all active:scale-95 shadow-xl hover:shadow-primary/20 bg-primary hover:bg-primary/90 text-primary-foreground w-full"
                      disabled={!selectedTemplate || !boardName || isSubmitting}
                      onClick={handleConfirm}
                    >
                      {isSubmitting ? <Loader2 className="mr-2 h-6 w-6 animate-spin" /> : <Plus className="mr-2 h-6 w-6 stroke-[3px]" />}
                      Create Board
                    </Button>
                  </div>
               </div>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
