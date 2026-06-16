"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";
import { 
  Building2, Layout, ArrowRight, Check, Loader2, 
  Sparkles, Monitor, Network, Milestone 
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { apiClient } from "@/lib/api";
import { BoardTemplateResponse } from "@/types/template";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const STEPS = ["Workspace", "Template", "Finalizing"];

export default function OnboardingPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState(0);
  const [workspaceName, setWorkspaceName] = useState("");
  const [templates, setTemplates] = useState<BoardTemplateResponse[]>([]);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [isProcessing, setIsGenerating] = useState(false);

  useEffect(() => {
    const loadTemplates = async () => {
      try {
        const res = await apiClient.get<BoardTemplateResponse[]>("/templates?isSystem=true");
        setTemplates(res.data);
      } catch (err) {
        console.error("Failed to load templates");
      }
    };
    loadTemplates();
  }, []);

  const handleNextStep = () => {
    if (currentStep === 0 && !workspaceName.trim()) {
      toast.error("Please name your workspace");
      return;
    }
    setCurrentStep(prev => prev + 1);
  };

  const handleComplete = async () => {
    setIsGenerating(true);
    try {
      // 1. Create Workspace
      const wsRes = await apiClient.post("/workspaces", { name: workspaceName });
      const workspaceId = wsRes.data.id;

      // 2. Create Board (either from template or blank)
      let boardRes;
      if (selectedTemplateId) {
        boardRes = await apiClient.post(`/workspaces/${workspaceId}/boards/from-template`, {
          templateId: selectedTemplateId,
          name: "My First Board"
        });
      } else {
        boardRes = await apiClient.post(`/workspaces/${workspaceId}/boards`, {
          name: "My First Board",
          type: "Flowchart"
        });
      }

      toast.success("Welcome to FlowSpace!");
      router.push(`/boards/${boardRes.data.id}`);
    } catch (error) {
      toast.error("Something went wrong. Please try again.");
      setIsGenerating(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/30 p-4">
      <div className="w-full max-w-2xl">
        {/* Progress Bar */}
        <div className="mb-12 flex justify-between">
          {STEPS.map((step, i) => (
            <div key={step} className="flex flex-col items-center gap-2">
              <div className={cn(
                "flex h-8 w-8 items-center justify-center rounded-full text-xs font-bold transition-all",
                currentStep >= i ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground border"
              )}>
                {currentStep > i ? <Check className="h-4 w-4" /> : i + 1}
              </div>
              <span className={cn("text-[10px] font-black uppercase tracking-widest", currentStep >= i ? "text-primary" : "text-muted-foreground")}>
                {step}
              </span>
            </div>
          ))}
        </div>

        <Card className="border-2 shadow-2xl shadow-primary/5">
          <CardContent className="p-10">
            <AnimatePresence mode="wait">
              {currentStep === 0 && (
                <motion.div
                  key="step0"
                  initial={{ opacity: 0, x: 20 }}
                  animate={{ opacity: 1, x: 0 }}
                  exit={{ opacity: 0, x: -20 }}
                  className="space-y-6"
                >
                  <div className="space-y-2">
                    <h2 className="text-3xl font-black tracking-tight">Name your workspace</h2>
                    <p className="text-muted-foreground font-medium">This is where all your diagrams and whiteboards will live.</p>
                  </div>
                  <div className="space-y-4">
                    <div className="relative">
                        <Building2 className="absolute left-3 top-3 h-5 w-5 text-muted-foreground" />
                        <Input 
                            placeholder="e.g. Acme Engineering or My Projects" 
                            className="h-12 pl-10 text-lg font-bold border-2 focus-visible:ring-primary"
                            value={workspaceName}
                            onChange={(e) => setWorkspaceName(e.target.value)}
                            autoFocus
                        />
                    </div>
                    <Button className="w-full h-12 font-black uppercase tracking-wider gap-2" onClick={handleNextStep}>
                        Next: Choose Blueprint <ArrowRight className="h-4 w-4" />
                    </Button>
                  </div>
                </motion.div>
              )}

              {currentStep === 1 && (
                <motion.div
                  key="step1"
                  initial={{ opacity: 0, x: 20 }}
                  animate={{ opacity: 1, x: 0 }}
                  exit={{ opacity: 0, x: -20 }}
                  className="space-y-8"
                >
                  <div className="space-y-2 text-center">
                    <h2 className="text-3xl font-black tracking-tight">Choose a starting point</h2>
                    <p className="text-muted-foreground font-medium">Select a blueprint or start from a clean slate.</p>
                  </div>
                  
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <button
                        onClick={() => setSelectedTemplateId(null)}
                        className={cn(
                            "flex flex-col items-center gap-4 rounded-2xl border-4 p-6 transition-all hover:bg-muted/50",
                            selectedTemplateId === null ? "border-primary bg-primary/5" : "border-transparent bg-muted/20"
                        )}
                    >
                        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted border-2 shadow-inner">
                            <Monitor className="h-6 w-6 text-muted-foreground" />
                        </div>
                        <div className="flex flex-col">
                            <span className="font-black text-sm uppercase tracking-tight">Blank Board</span>
                            <span className="text-[10px] font-bold text-muted-foreground uppercase opacity-70">Start from zero</span>
                        </div>
                    </button>

                    {templates.slice(0, 3).map((t) => (
                        <button
                            key={t.id}
                            onClick={() => setSelectedTemplateId(t.id)}
                            className={cn(
                                "flex flex-col items-center gap-4 rounded-2xl border-4 p-6 transition-all hover:bg-muted/50",
                                selectedTemplateId === t.id ? "border-primary bg-primary/5" : "border-transparent bg-muted/20"
                            )}
                        >
                            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10 border-2 border-primary/20 shadow-lg shadow-primary/5">
                                {t.boardType === 'Flowchart' && <Network className="h-6 w-6 text-primary" />}
                                {t.boardType === 'Whiteboard' && <Layout className="h-6 w-6 text-primary" />}
                                {t.boardType === 'Mindmap' && <Sparkles className="h-6 w-6 text-primary" />}
                            </div>
                            <div className="flex flex-col">
                                <span className="font-black text-sm uppercase tracking-tight">{t.name}</span>
                                <span className="text-[10px] font-bold text-primary uppercase">{t.boardType} Blueprint</span>
                            </div>
                        </button>
                    ))}
                  </div>

                  <div className="flex gap-4">
                    <Button variant="ghost" className="h-12 font-bold uppercase tracking-wider flex-1" onClick={() => setCurrentStep(0)}>
                        Back
                    </Button>
                    <Button className="h-12 font-black uppercase tracking-wider flex-[2] gap-2 shadow-xl" onClick={handleComplete} disabled={isProcessing}>
                        {isProcessing ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
                        Let's Go!
                    </Button>
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </CardContent>
        </Card>

        {isProcessing && (
            <div className="mt-8 text-center animate-pulse">
                <p className="text-xs font-black uppercase tracking-[0.3em] text-primary">Constructing your visual universe...</p>
            </div>
        )}
      </div>
    </div>
  );
}
