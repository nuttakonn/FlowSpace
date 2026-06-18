"use client";

import React, { useState } from 'react';
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
import { ScrollArea } from '@/components/ui/scroll-area';
import { Loader2, LayoutTemplate } from 'lucide-react';
import { cn } from '@/lib/utils';

interface Template {
  id: string;
  name: string;
  description: string;
  category: string;
  boardType: string;
}

const TEMPLATES: Template[] = [
  { id: 'blank-flowchart', name: 'Blank Board', description: 'Start with a clean canvas.', category: 'All', boardType: 'Flowchart' },
  { id: 'simple-flowchart', name: 'Simple Flowchart', description: 'Standard process flow template.', category: 'Engineering', boardType: 'Flowchart' },
  { id: 'system-architecture', name: 'System Architecture', description: 'Plan your cloud or software infrastructure.', category: 'Engineering', boardType: 'Flowchart' },
  { id: 'brainstorming', name: 'Brainstorming Session', description: 'Collaborative space for team ideas.', category: 'Product & Design', boardType: 'Flowchart' },
  { id: 'org-chart', name: 'Organization Chart', description: 'Map out team structure and reporting.', category: 'Business & Management', boardType: 'Flowchart' },
];

const CATEGORIES = ['All', 'Engineering', 'Product & Design', 'Business & Management'];

interface TemplateGalleryProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSelect: (template: Template, name: string) => Promise<void>;
  isSubmitting?: boolean;
}

export function TemplateGallery({ isOpen, onOpenChange, onSelect, isSubmitting }: TemplateGalleryProps) {
  const [selectedTemplate, setSelectedTemplate] = useState<Template | null>(TEMPLATES[0]);
  const [activeCategory, setActiveCategory] = useState('All');
  const [boardName, setBoardName] = useState('');

  const handleConfirm = () => {
    if (selectedTemplate && boardName) {
      onSelect(selectedTemplate, boardName);
    }
  };

  const filteredTemplates = TEMPLATES.filter(t => 
    activeCategory === 'All' || t.category === activeCategory || t.id === 'blank-flowchart'
  );

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Create New Board</DialogTitle>
          <DialogDescription>
            Choose a starting template and give your board a name.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="space-y-1.5">
            <label className="text-sm font-semibold tracking-tight">Board Name</label>
            <Input 
              placeholder="e.g. Q4 Strategy" 
              value={boardName}
              onChange={(e) => setBoardName(e.target.value)}
              autoFocus
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm font-semibold tracking-tight">Select Template</label>
            
            {/* Category Filter Tabs */}
            <div className="flex flex-wrap gap-1 bg-muted/50 p-1 rounded-lg border text-xs">
              {CATEGORIES.map((cat) => (
                <button
                  key={cat}
                  type="button"
                  onClick={() => setActiveCategory(cat)}
                  className={cn(
                    "px-2.5 py-1 rounded-md font-medium transition-all",
                    activeCategory === cat ? "bg-background text-foreground shadow-sm font-bold" : "text-muted-foreground hover:text-foreground"
                  )}
                >
                  {cat}
                </button>
              ))}
            </div>

            <ScrollArea className="h-48 rounded-md border p-2 bg-muted/10">
              <div className="flex flex-col gap-1.5">
                {filteredTemplates.map((template) => (
                  <button
                    key={template.id}
                    onClick={() => {
                        setSelectedTemplate(template);
                        if (!boardName) setBoardName(template.name);
                    }}
                    className={cn(
                      "flex items-start gap-3 rounded-lg p-2.5 text-left transition-colors border",
                      selectedTemplate?.id === template.id ? "bg-primary/10 border-primary/40" : "hover:bg-background border-transparent"
                    )}
                  >
                    <LayoutTemplate className={cn("h-4 w-4 mt-0.5 shrink-0", selectedTemplate?.id === template.id ? "text-primary" : "text-muted-foreground")} />
                    <div className="flex flex-col gap-0.5">
                      <span className="text-xs font-bold leading-none flex items-center gap-2">
                        {template.name}
                        {template.category !== 'All' && (
                          <span className="text-[9px] font-semibold px-1 py-0.5 bg-muted rounded text-muted-foreground uppercase tracking-tight scale-90">{template.category}</span>
                        )}
                      </span>
                      <span className="text-[11px] text-muted-foreground leading-normal">{template.description}</span>
                    </div>
                  </button>
                ))}
              </div>
            </ScrollArea>
          </div>
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
      </DialogContent>
    </Dialog>
  );
}
