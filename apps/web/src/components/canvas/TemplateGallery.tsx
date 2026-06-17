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
      <DialogContent className="max-w-4xl h-[85vh] flex flex-col p-0 overflow-hidden gap-0">
        <div className="flex flex-1 min-h-0">
          {/* Sidebar */}
          <div className="w-64 border-r bg-muted/30 p-4 flex flex-col gap-4">
            <div className="relative">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input 
                placeholder="Search templates..." 
                className="pl-8 h-9" 
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            
            <div className="space-y-1">
              <p className="px-2 text-[10px] font-bold uppercase tracking-wider text-muted-foreground py-2">Categories</p>
              {CATEGORIES.map(cat => (
                <Button
                  key={cat}
                  variant={selectedCategory === cat ? "secondary" : "ghost"}
                  className="w-full justify-start h-9 px-2"
                  onClick={() => setSelectedCategory(cat)}
                >
                  {cat}
                </Button>
              ))}
            </div>
          </div>

          {/* Main Gallery */}
          <div className="flex-1 flex flex-col min-w-0">
            <DialogHeader className="p-6 pb-2">
              <DialogTitle>Choose a template</DialogTitle>
              <DialogDescription>Start from scratch or use a pre-built structure.</DialogDescription>
            </DialogHeader>

            <ScrollArea className="flex-1 p-6 pt-2">
              <div className="grid grid-cols-2 gap-4">
                {filteredTemplates.map((template) => (
                  <div
                    key={template.id}
                    className={cn(
                      "group relative flex flex-col gap-3 rounded-xl border p-4 cursor-pointer transition-all hover:border-primary/50 hover:shadow-md",
                      selectedTemplate?.id === template.id ? "border-primary bg-primary/5 ring-1 ring-primary" : "bg-card"
                    )}
                    onClick={() => {
                        setSelectedTemplate(template);
                        if (!boardName) setBoardName(template.name);
                    }}
                  >
                    <div className={cn("h-32 w-full rounded-lg mb-1 flex items-center justify-center relative overflow-hidden", template.previewColor)}>
                        <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity" />
                        <template.icon className="h-12 w-12 text-white/90 drop-shadow-lg" />
                    </div>
                    <div>
                        <h3 className="font-semibold text-sm leading-none mb-2">{template.name}</h3>
                        <p className="text-xs text-muted-foreground line-clamp-1">{template.description}</p>
                    </div>
                    {selectedTemplate?.id === template.id && (
                        <div className="absolute top-2 right-2 h-5 w-5 bg-primary rounded-full flex items-center justify-center">
                            <Check className="h-3 w-3 text-white" />
                        </div>
                    )}
                  </div>
                ))}
              </div>
            </ScrollArea>

            <div className="p-6 border-t bg-muted/10 space-y-4">
               <div className="space-y-2">
                 <label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Board Name</label>
                 <Input 
                   placeholder="e.g. Q4 Strategy" 
                   value={boardName}
                   onChange={(e) => setBoardName(e.target.value)}
                   className="h-10"
                   autoFocus
                 />
               </div>
               <DialogFooter>
                 <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                 <Button 
                   disabled={!selectedTemplate || !boardName || isSubmitting}
                   onClick={handleConfirm}
                 >
                   {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                   Create Board
                 </Button>
               </DialogFooter>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
