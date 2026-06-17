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
      <DialogContent className="max-w-[1200px] w-[95vw] h-[90vh] md:h-[85vh] flex flex-col p-0 gap-0 overflow-hidden">
        <DialogTitle className="sr-only">Choose a template</DialogTitle>
        <DialogDescription className="sr-only">Select a template to kickstart your workflow.</DialogDescription>
        <div className="flex flex-col md:flex-row h-full min-h-0 overflow-hidden">
          {/* Sidebar */}
          <aside className="w-full md:w-64 lg:w-72 border-b md:border-b-0 md:border-r bg-muted/20 flex flex-col flex-shrink-0 max-h-[40vh] md:max-h-none">
            <div className="p-4 md:p-6 pb-2 hidden md:block">
              <h2 className="text-xl font-bold tracking-tight">FlowSpace</h2>
              <p className="text-[10px] text-muted-foreground uppercase font-bold tracking-widest mt-1">Template Library</p>
            </div>
            
            <div className="p-4 md:pt-2">
              <div className="relative">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input 
                  placeholder="Search templates..." 
                  className="pl-9 h-9 bg-background" 
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
            </div>

            <ScrollArea className="flex-1 px-4 md:block">
              <div className="flex md:flex-col gap-2 md:gap-1 py-2 overflow-x-auto pb-4 md:pb-2">
                <p className="hidden md:block px-2 text-[10px] font-bold uppercase tracking-wider text-muted-foreground py-2">Categories</p>
                {CATEGORIES.map(cat => (
                  <Button
                    key={cat}
                    variant={selectedCategory === cat ? "secondary" : "ghost"}
                    className={cn(
                      "flex-shrink-0 md:w-full justify-start h-10 px-3 md:px-2 gap-2 md:gap-3 transition-colors rounded-full md:rounded-md whitespace-nowrap border md:border-transparent",
                      selectedCategory === cat ? "bg-primary/10 text-primary hover:bg-primary/15 border-primary/20" : "border-border"
                    )}
                    onClick={() => setSelectedCategory(cat)}
                  >
                    <span className="hidden md:inline-flex">
                      {cat === 'All' && <Layout className="h-4 w-4" />}
                      {cat === 'Whiteboarding' && <Layers className="h-4 w-4" />}
                      {cat === 'Diagrams' && <Workflow className="h-4 w-4" />}
                      {cat === 'Architecture' && <Database className="h-4 w-4" />}
                      {cat === 'Management' && <Users className="h-4 w-4" />}
                    </span>
                    <span className="text-sm font-medium">{cat}</span>
                  </Button>
                ))}
              </div>
            </ScrollArea>

            <div className="p-4 border-t bg-muted/10 hidden md:block">
              <Button variant="ghost" className="w-full justify-start gap-3 h-10 text-muted-foreground" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
            </div>
          </aside>

          {/* Main Gallery */}
          <main className="flex-1 flex flex-col min-w-0 bg-background overflow-hidden">
            <div className="p-4 md:p-8 pb-2 md:pb-4 flex-shrink-0">
              <h1 className="text-xl md:text-2xl font-bold tracking-tight">Choose a template</h1>
              <p className="text-sm md:text-base text-muted-foreground">Kickstart your workflow with a pre-built canvas.</p>
            </div>

            <ScrollArea className="flex-1 px-4 md:px-8 py-2 md:py-4">
              <motion.div 
                layout
                className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6 pb-4 md:pb-8 min-w-0"
              >
                <AnimatePresence>
                  {filteredTemplates.map((template) => (
                    <motion.div
                      key={template.id}
                      layout
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, scale: 0.95 }}
                      whileHover={{ y: -4 }}
                      className={cn(
                        "group relative flex flex-col rounded-2xl border-2 cursor-pointer transition-all duration-300",
                        selectedTemplate?.id === template.id ? "border-primary bg-primary/5 shadow-lg shadow-primary/5" : "bg-card hover:border-primary/30 hover:shadow-xl"
                      )}
                      onClick={() => {
                          setSelectedTemplate(template);
                          setBoardName(prev => prev || template.name);
                      }}
                    >
                      <div className={cn("aspect-video w-full rounded-t-xl flex items-center justify-center relative overflow-hidden flex-shrink-0", template.previewColor)}>
                          <div className="absolute inset-0 bg-black/10 opacity-0 group-hover:opacity-100 transition-opacity" />
                          <template.icon className="h-12 w-12 md:h-16 md:w-12 text-white/90 drop-shadow-lg md:drop-shadow-2xl md:scale-125" />
                      </div>
                      <div className="p-4 md:p-5 flex-1 bg-card rounded-b-xl">
                          <h3 className="font-bold text-sm md:text-base leading-none mb-1 md:mb-2 text-foreground">{template.name}</h3>
                          <p className="text-[10px] md:text-xs text-muted-foreground leading-relaxed line-clamp-2">{template.description}</p>
                      </div>
                      {selectedTemplate?.id === template.id && (
                          <div className="absolute top-2 right-2 md:top-3 md:right-3 h-5 w-5 md:h-6 md:w-6 bg-primary rounded-full flex items-center justify-center shadow-lg">
                              <Check className="h-3 w-3 md:h-3.5 md:w-3.5 text-white" />
                          </div>
                      )}
                    </motion.div>
                  ))}
                </AnimatePresence>
              </motion.div>
            </ScrollArea>

            {/* Selection UI */}
            <div className="p-4 md:p-8 border-t bg-muted/5 flex flex-col sm:flex-row items-stretch sm:items-end gap-4 md:gap-6 shadow-[0_-4px_20px_-4px_rgba(0,0,0,0.05)] flex-shrink-0">
               <div className="flex-1 space-y-2 md:space-y-2.5">
                 <label className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest ml-1">Board Name</label>
                 <Input 
                   placeholder="e.g. Q4 Strategy" 
                   value={boardName}
                   onChange={(e) => setBoardName(e.target.value)}
                   className="h-10 md:h-12 text-base md:text-lg font-medium bg-background border-2 focus-visible:ring-offset-0 focus-visible:ring-primary/20"
                   autoFocus
                 />
               </div>
               <div className="flex gap-3 justify-end mt-2 sm:mt-0">
                 <Button variant="ghost" className="md:hidden" onClick={() => onOpenChange(false)}>Cancel</Button>
                 <Button 
                   size="lg"
                   className="h-10 md:h-12 px-6 md:px-8 font-bold text-sm md:text-base transition-all active:scale-95 w-full sm:w-auto"
                   disabled={!selectedTemplate || !boardName || isSubmitting}
                   onClick={handleConfirm}
                 >
                   {isSubmitting && <Loader2 className="mr-2 h-4 w-4 md:h-5 md:w-5 animate-spin" />}
                   Create Board
                 </Button>
               </div>
            </div>
          </main>
        </div>
      </DialogContent>
    </Dialog>
  );
}
