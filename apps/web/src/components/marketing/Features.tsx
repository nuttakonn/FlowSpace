"use client";

import { CheckCircle2, Cloud, Users, Zap, Layout, Infinity, History, Download } from "lucide-react";

const features = [
  {
    name: "Gemini-Powered Generation",
    description: "Generate complex flowcharts, mindmaps, and system architectures from simple natural language prompts.",
    icon: Zap,
    color: "bg-amber-100 text-amber-600",
  },
  {
    name: "Real-time Collaboration",
    description: "Built on SignalR and Yjs. See cursors, edits, and ideas sync instantly across the entire team.",
    icon: Users,
    color: "bg-blue-100 text-blue-600",
  },
  {
    name: "Infinite Whiteboard",
    description: "A free-form space powered by tldraw for brainstorming, wireframing, and creative exploration.",
    icon: Infinity,
    color: "bg-emerald-100 text-emerald-600",
  },
  {
    name: "Version Control",
    description: "Capture snapshots of your progress. Restore any previous version of your board with full CRDT fidelity.",
    icon: History,
    color: "bg-purple-100 text-purple-600",
  },
  {
    name: "Template Marketplace",
    description: "Start from high-quality community blueprints or save your own board as a reusable team template.",
    icon: Layout,
    color: "bg-pink-100 text-pink-600",
  },
  {
    name: "Pro Export & Interop",
    description: "Export to high-fidelity PDF, SVG, PNG or even draw.io XML. Never feel locked into a single tool.",
    icon: Download,
    color: "bg-cyan-100 text-cyan-600",
  },
];

export function Features() {
  return (
    <section id="features" className="py-24 bg-muted/30">
      <div className="container mx-auto px-4">
        <div className="text-center mb-16">
          <h2 className="text-sm font-bold uppercase tracking-[0.2em] text-primary mb-4">Engineering Standards</h2>
          <h3 className="text-4xl font-black tracking-tight sm:text-5xl mb-6">Built for scale, designed for flow.</h3>
          <p className="text-lg text-muted-foreground font-medium max-w-2xl mx-auto leading-relaxed">
            FlowSpace combines industry-standard diagramming with modern AI orchestration 
            to help you ship faster and communicate better.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {features.map((feature) => (
            <div key={feature.name} className="group relative rounded-2xl border bg-background p-8 transition-all hover:shadow-xl hover:shadow-primary/5 hover:border-primary/20">
              <div className={`mb-6 inline-flex h-12 w-12 items-center justify-center rounded-xl ${feature.color}`}>
                <feature.icon className="h-6 w-6" />
              </div>
              <h4 className="mb-3 text-xl font-bold tracking-tight">{feature.name}</h4>
              <p className="text-muted-foreground font-medium leading-relaxed">
                {feature.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
