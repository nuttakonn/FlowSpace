"use client";

import { useState, useEffect } from "react";
import { useCanvasStore } from "@/store/useCanvasStore";
import { 
  Sparkles, Send, Loader2, X, Check, RotateCcw, 
  Layout, Network, Users, Milestone, ArrowRight, History, Play, Eye, EyeOff,
  Zap, Eraser, HelpCircle
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { 
  Sheet, 
  SheetContent, 
  SheetHeader, 
  SheetTitle, 
  SheetTrigger 
} from "@/components/ui/sheet";
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { cn } from "@/lib/utils";

const AI_TYPES = [
  { id: "Flowchart", label: "Flowchart", icon: Network },
  { id: "Mindmap", label: "Mindmap", icon: Layout },
  { id: "SystemArchitecture", label: "Architecture", icon: Users },
  { id: "UserJourney", label: "User Journey", icon: Milestone },
];

const REFINEMENT_COMMANDS = [
  { id: "ImproveArchitecture", label: "Improve Architecture", icon: Zap, color: "text-amber-500" },
  { id: "SimplifyDiagram", label: "Simplify Diagram", icon: Eraser, color: "text-emerald-500" },
  { id: "ExplainDiagram", label: "Explain Diagram", icon: HelpCircle, color: "text-blue-500" },
];

const STEPS = ["Analyzing current context...", "Consulting Gemini 1.5...", "Generating improvements...", "Optimizing layout...", "Finalizing diff preview..."];

export function AiSidePanel() {
  const { 
    previewAiDiagram, acceptAiPreview, rejectAiPreview, isGeneratingAi, 
    previewNodes, aiHistory, fetchAiHistory, templates, fetchTemplates,
    showCurrentInPreview, togglePreviewComparison, nodes
  } = useCanvasStore();

  const [isOpen, setIsOpen] = useState(false);
  const [prompt, setPrompt] = useState("");
  const [type, setType] = useState("Flowchart");
  const [templateId, setTemplateId] = useState<string>("none");
  const [progress, setProgress] = useState(0);
  const [currentStep, setCurrentStep] = useState(0);
  const [activeTab, setActiveTab] = useState("generate");

  useEffect(() => { if (isOpen) { fetchAiHistory(); fetchTemplates(); } }, [isOpen, fetchAiHistory, fetchTemplates]);

  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (isGeneratingAi) {
      setProgress(0); setCurrentStep(0);
      interval = setInterval(() => {
        setProgress(prev => {
          if (prev >= 95) return prev;
          const next = prev + Math.random() * 15;
          setCurrentStep(Math.floor((next / 100) * STEPS.length));
          return next;
        });
      }, 800);
    } else { setProgress(100); }
    return () => clearInterval(interval);
  }, [isGeneratingAi]);

  const handleGenerate = async () => {
    if (!prompt.trim()) return;
    await previewAiDiagram(prompt, type, templateId === "none" ? undefined : templateId);
  };

  const hasPreview = previewNodes.length > 0;

  return (
    <Sheet open={isOpen} onOpenChange={setIsOpen}>
      <SheetTrigger asChild>
        <Button variant="default" size="sm" className="gap-2 shadow-md hover:shadow-lg transition-all font-bold uppercase tracking-wider">
          <Sparkles className="h-4 w-4" /> AI Assistant
        </Button>
      </SheetTrigger>
      <SheetContent side="right" className="w-[400px] sm:w-[500px] flex flex-col gap-0 p-0">
        <div className="p-6 pb-2">
          <SheetHeader>
            <SheetTitle className="flex items-center gap-2 font-bold uppercase tracking-tight">
              <Sparkles className="h-5 w-5 text-primary" /> AI Diagram Expert
            </SheetTitle>
          </SheetHeader>
        </div>

        <Tabs value={activeTab} onValueChange={setActiveTab} className="flex-1 flex flex-col h-full overflow-hidden">
          <div className="px-6 border-b">
            <TabsList className="grid w-full grid-cols-2 h-10">
              <TabsTrigger value="generate" className="text-[10px] font-bold uppercase tracking-widest">Generate & Refine</TabsTrigger>
              <TabsTrigger value="history" className="text-[10px] font-bold uppercase tracking-widest">History</TabsTrigger>
            </TabsList>
          </div>

          <TabsContent value="generate" className="flex-1 flex flex-col gap-6 p-6 overflow-y-auto m-0">
            {nodes.length > 0 && (
              <div className="space-y-3 p-4 rounded-xl bg-muted/30 border border-dashed">
                <label className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">Board Refinement Actions</label>
                <div className="grid grid-cols-1 gap-2">
                  {REFINEMENT_COMMANDS.map((cmd) => (
                    <Button
                      key={cmd.id}
                      variant="outline"
                      size="sm"
                      className="justify-start px-3 h-10 border-2 hover:border-primary/50 group"
                      onClick={() => previewAiDiagram(prompt || `Refine this board: ${cmd.label}`, type, undefined, cmd.id)}
                      disabled={isGeneratingAi || hasPreview}
                    >
                      <cmd.icon className={cn("mr-2 h-4 w-4 group-hover:scale-110 transition-transform", cmd.color)} />
                      <span className="text-xs font-bold uppercase tracking-tight">{cmd.label}</span>
                    </Button>
                  ))}
                </div>
              </div>
            )}

            <div className="space-y-3">
              <label className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">New Generation Context</label>
              <Select value={templateId} onValueChange={setTemplateId} disabled={isGeneratingAi || hasPreview}>
                <SelectTrigger className="w-full h-10 border-2">
                  <SelectValue placeholder="Select a template..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none" className="text-xs font-medium">Clean Slate (Default)</SelectItem>
                  {templates.map((t) => (
                    <SelectItem key={t.id} value={t.id} className="text-xs font-medium">{t.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-3">
              <label className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">Visual Style</label>
              <div className="grid grid-cols-2 gap-2">
                {AI_TYPES.map((t) => (
                  <Button key={t.id} variant={type === t.id ? "default" : "outline"} size="sm" className={cn("justify-start px-3 h-10 border-2", type === t.id ? "border-primary" : "border-transparent")} onClick={() => setType(t.id)} disabled={isGeneratingAi || hasPreview}>
                    <t.icon className="mr-2 h-4 w-4" />
                    <span className="text-xs font-bold tracking-tight">{t.label}</span>
                  </Button>
                ))}
              </div>
            </div>

            <div className="space-y-3">
              <label className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">Describe Your Goal</label>
              <Textarea placeholder="e.g., 'Visualize a serverless payment gateway'..." className="min-h-[120px] resize-none text-sm font-medium leading-relaxed border-2" value={prompt} onChange={(e) => setPrompt(e.target.value)} disabled={isGeneratingAi} />
            </div>

            {isGeneratingAi && (
              <div className="space-y-3 rounded-xl border-2 border-primary/20 bg-primary/5 p-4 animate-in fade-in zoom-in-95">
                <div className="flex items-center justify-between text-[10px] font-bold uppercase tracking-widest">
                  <span className="flex items-center gap-2"><Loader2 className="h-3 w-3 animate-spin text-primary" />{STEPS[currentStep] || "Processing..."}</span>
                  <span className="text-primary">{Math.round(progress)}%</span>
                </div>
                <Progress value={progress} className="h-1.5" />
              </div>
            )}

            {hasPreview && !isGeneratingAi && (
              <div className="space-y-4 rounded-xl border-2 border-primary bg-primary/5 p-6 animate-in slide-in-from-top-4 shadow-xl">
                <div className="text-center space-y-2">
                  <div className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-primary text-primary-foreground shadow-lg"><Check className="h-6 w-6" /></div>
                  <h3 className="text-sm font-black uppercase tracking-tighter">AI Proposal Ready</h3>
                  <p className="text-[10px] text-muted-foreground font-bold leading-tight">Review the dashed blue elements on your canvas. You can edit them before applying.</p>
                </div>

                <div className="flex flex-col gap-2">
                  <Button variant="secondary" size="sm" className="w-full h-10 text-[10px] font-black uppercase tracking-widest gap-2 shadow-sm border-2 border-primary/20" onClick={togglePreviewComparison}>
                    {showCurrentInPreview ? <><EyeOff className="h-4 w-4" /> Hide Current Board</> : <><Eye className="h-4 w-4" /> Show Current Board</>}
                  </Button>
                </div>

                <div className="grid grid-cols-2 gap-3 pt-2">
                  <Button variant="outline" className="w-full h-12 text-xs font-black uppercase tracking-widest border-2 hover:bg-destructive hover:text-white transition-colors" onClick={rejectAiPreview}>Discard</Button>
                  <Button className="w-full h-12 text-xs font-black uppercase tracking-widest shadow-xl" onClick={acceptAiPreview}>Apply All</Button>
                </div>
              </div>
            )}

            <div className="mt-auto pt-6 border-t">
              <Button className="w-full h-14 text-sm font-black uppercase tracking-[0.1em] group shadow-2xl transition-all active:scale-[0.98]" onClick={handleGenerate} disabled={isGeneratingAi || !prompt.trim() || hasPreview}>
                {isGeneratingAi ? <><Loader2 className="mr-2 h-5 w-5 animate-spin" />Consulting AI...</> : <><Send className="mr-2 h-5 w-5 transition-transform group-hover:translate-x-1 group-hover:-translate-y-1" />Generate Diagram<ArrowRight className="ml-auto h-5 w-5 opacity-50" /></>}
              </Button>
              <div className="mt-4 flex items-center justify-center gap-2 text-[9px] text-muted-foreground uppercase font-black tracking-[0.2em] opacity-60">
                <Badge variant="outline" className="text-[8px] px-1.5 py-0 border-primary text-primary font-black">Gemini 1.5 PRO</Badge>
                Intelligence Engine
              </div>
            </div>
          </TabsContent>

          <TabsContent value="history" className="flex-1 flex flex-col p-6 overflow-hidden m-0">
            <ScrollArea className="h-full pr-4">
              {aiHistory.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-40 text-muted-foreground gap-4 opacity-30">
                  <History className="h-12 w-12" />
                  <p className="text-[10px] font-black uppercase tracking-widest">No Intelligence Records</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {aiHistory.map((h) => (
                    <div key={h.id} className="group flex flex-col gap-3 rounded-xl border-2 p-4 hover:border-primary transition-all bg-muted/5">
                      <div className="flex items-start justify-between">
                        <Badge variant="secondary" className="text-[9px] font-black uppercase tracking-widest h-5 px-2">{h.diagramType}</Badge>
                        <span className="text-[9px] text-muted-foreground font-bold">{new Date(h.createdAt).toLocaleDateString()}</span>
                      </div>
                      <p className="text-xs leading-relaxed line-clamp-3 italic font-bold text-foreground/80">"{h.prompt}"</p>
                      <div className="flex gap-2 pt-3 border-t-2 border-dashed mt-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button variant="ghost" size="sm" className="h-9 text-[10px] font-black uppercase tracking-widest border border-transparent hover:border-primary/20" onClick={() => { setPrompt(h.prompt); setType(h.diagramType); setActiveTab("generate"); }}>
                          <Play className="mr-2 h-3.5 w-3.5 text-primary" /> Re-run
                        </Button>
                        <Button variant="secondary" size="sm" className="h-9 text-[10px] font-black uppercase tracking-widest ml-auto shadow-sm" onClick={() => { 
                          useCanvasStore.setState({ 
                            previewNodes: h.result.nodes.map(n => ({ id: `preview-${n.id}`, type: n.type.toLowerCase(), position: { x: n.x, y: n.y }, data: { label: n.type, metadata: n.metadata }, style: { opacity: 0.8, border: '2px dashed #3b82f6', background: '#eff6ff' } })),
                            previewEdges: h.result.edges.map(e => ({ id: `preview-${e.id}`, source: `preview-${e.sourceNodeId}`, target: `preview-${e.targetNodeId}`, animated: true, style: { stroke: '#3b82f6', strokeDasharray: '5,5' } }))
                          });
                          setActiveTab("generate");
                        }}>
                          Restore
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </ScrollArea>
          </TabsContent>
        </Tabs>
      </SheetContent>
    </Sheet>
  );
}
