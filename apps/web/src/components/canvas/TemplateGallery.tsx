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
  { id: 'simple-flowchart', name: 'Simple Flowchart', description: 'Standard process flow template.', category: 'Diagrams', boardType: 'Flowchart' },
  { id: 'system-architecture', name: 'System Architecture', description: 'Plan your cloud or software infrastructure.', category: 'Architecture', boardType: 'Flowchart' },
  { id: 'brainstorming', name: 'Brainstorming Session', description: 'Collaborative space for team ideas.', category: 'Whiteboarding', boardType: 'Flowchart' },
  { id: 'org-chart', name: 'Organization Chart', description: 'Map out team structure and reporting.', category: 'Management', boardType: 'Flowchart' },
];

interface TemplateGalleryProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onSelect: (template: Template, name: string) => Promise<void>;
  isSubmitting?: boolean;
}

export function TemplateGallery({ isOpen, onOpenChange, onSelect, isSubmitting }: TemplateGalleryProps) {
  const [selectedTemplate, setSelectedTemplate] = useState<Template | null>(TEMPLATES[0]);
  const [boardName, setBoardName] = useState('');

  const handleConfirm = () => {
    if (selectedTemplate && boardName) {
      onSelect(selectedTemplate, boardName);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Create New Board</DialogTitle>
          <DialogDescription>
            Choose a starting template and give your board a name.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          <div className="space-y-2">
            <label className="text-sm font-medium">Board Name</label>
            <Input 
              placeholder="e.g. Q4 Strategy" 
              value={boardName}
              onChange={(e) => setBoardName(e.target.value)}
              autoFocus
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium">Select Template</label>
            <ScrollArea className="h-48 rounded-md border p-2">
              <div className="flex flex-col gap-2">
                {TEMPLATES.map((template) => (
                  <button
                    key={template.id}
                    onClick={() => {
                        setSelectedTemplate(template);
                        if (!boardName) setBoardName(template.name);
                    }}
                    className={cn(
                      "flex items-start gap-3 rounded-lg p-3 text-left transition-colors",
                      selectedTemplate?.id === template.id ? "bg-primary/10 border-primary border" : "hover:bg-muted border border-transparent"
                    )}
                  >
                    <LayoutTemplate className={cn("h-5 w-5 mt-0.5", selectedTemplate?.id === template.id ? "text-primary" : "text-muted-foreground")} />
                    <div className="flex flex-col gap-1">
                      <span className="text-sm font-medium leading-none">{template.name}</span>
                      <span className="text-xs text-muted-foreground">{template.description}</span>
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
