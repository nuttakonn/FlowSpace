"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { motion } from "framer-motion";
import { Sparkles, ArrowRight, MousePointer2 } from "lucide-react";

export function Hero() {
  return (
    <section className="relative overflow-hidden pt-20 pb-32 md:pt-32 md:pb-48">
      {/* Background Gradients */}
      <div className="absolute top-0 -z-10 h-full w-full bg-white">
        <div className="absolute bottom-auto left-auto right-0 top-0 h-[500px] w-[500px] -translate-x-[30%] translate-y-[20%] rounded-full bg-[rgba(173,109,244,0.15)] opacity-50 blur-[80px]"></div>
        <div className="absolute bottom-auto left-0 right-auto top-0 h-[500px] w-[500px] translate-x-[20%] translate-y-[10%] rounded-full bg-[rgba(59,130,246,0.15)] opacity-50 blur-[80px]"></div>
      </div>

      <div className="container mx-auto px-4 text-center">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="mx-auto max-w-4xl"
        >
          <div className="mb-6 flex justify-center">
            <span className="inline-flex items-center gap-2 rounded-full bg-primary/10 px-4 py-1.5 text-sm font-bold text-primary ring-1 ring-inset ring-primary/20">
              <Sparkles className="h-4 w-4" /> 
              Meet Gemini-powered Visual Logic
            </span>
          </div>
          <h1 className="mb-8 text-5xl font-black tracking-tight sm:text-7xl">
            Architect your ideas at the <span className="bg-gradient-to-r from-primary to-purple-600 bg-clip-text text-transparent">speed of thought.</span>
          </h1>
          <p className="mb-10 text-xl text-muted-foreground sm:text-2xl font-medium leading-relaxed max-w-2xl mx-auto">
            The collaborative visual workspace where AI builds your diagrams, 
            teams edit in real-time, and complexity becomes clarity.
          </p>
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
            <Link href="/register">
              <Button size="lg" className="h-14 px-8 text-lg font-black shadow-2xl shadow-primary/30 group">
                Build My First Board
                <ArrowRight className="ml-2 h-5 w-5 transition-transform group-hover:translate-x-1" />
              </Button>
            </Link>
            <Link href="/login">
              <Button size="lg" variant="outline" className="h-14 px-8 text-lg font-bold border-2">
                Watch Demo
              </Button>
            </Link>
          </div>
        </motion.div>

        {/* Product Preview Mockup */}
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 0.2 }}
          className="mt-20 relative mx-auto max-w-5xl rounded-2xl border-4 border-muted bg-muted p-2 shadow-2xl overflow-hidden"
        >
          <div className="bg-background rounded-lg h-[400px] md:h-[600px] w-full relative overflow-hidden flex flex-col">
              <div className="flex h-10 items-center px-4 border-b bg-muted/30 justify-between">
                <div className="flex gap-1.5">
                    <div className="h-2.5 w-2.5 rounded-full bg-red-400" />
                    <div className="h-2.5 w-2.5 rounded-full bg-amber-400" />
                    <div className="h-2.5 w-2.5 rounded-full bg-emerald-400" />
                </div>
                <div className="bg-muted px-3 py-1 rounded text-[10px] font-bold text-muted-foreground uppercase tracking-widest">
                    board-editor.flowspace.app
                </div>
                <div />
              </div>
              <div className="flex-1 bg-[radial-gradient(#e5e7eb_1px,transparent_1px)] [background-size:24px_24px] relative">
                 {/* Fake nodes */}
                 <div className="absolute top-20 left-1/2 -translate-x-1/2 w-48 h-12 bg-primary rounded border shadow flex items-center justify-center text-primary-foreground font-bold text-xs">
                    Input: "Create a microservice flow"
                 </div>
                 <div className="absolute top-44 left-1/2 -translate-x-1/2 flex gap-8">
                    <div className="w-32 h-20 border-2 border-dashed border-primary bg-primary/5 rounded flex items-center justify-center text-[10px] font-bold text-primary animate-pulse">
                        API Gateway
                    </div>
                    <div className="w-32 h-20 border-2 border-dashed border-primary bg-primary/5 rounded flex items-center justify-center text-[10px] font-bold text-primary animate-pulse">
                        Auth Service
                    </div>
                 </div>
                 
                 {/* Fake cursors */}
                 <div className="absolute top-60 left-1/3 flex items-center gap-1 transition-all">
                    <MousePointer2 className="h-4 w-4 fill-blue-500 text-blue-500" />
                    <span className="bg-blue-500 text-white text-[8px] px-1 rounded font-bold">Sarah</span>
                 </div>
                 <div className="absolute top-32 right-1/4 flex items-center gap-1">
                    <MousePointer2 className="h-4 w-4 fill-purple-500 text-purple-500" />
                    <span className="bg-purple-500 text-white text-[8px] px-1 rounded font-bold">Alex</span>
                 </div>
              </div>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
