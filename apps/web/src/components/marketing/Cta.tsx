"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { ArrowRight, Sparkles } from "lucide-react";

export function Cta() {
  return (
    <section className="py-24">
      <div className="container mx-auto px-4">
        <div className="relative overflow-hidden rounded-3xl bg-primary px-8 py-16 text-center text-primary-foreground shadow-2xl sm:px-16">
          {/* Decor */}
          <div className="absolute left-0 top-0 h-full w-full opacity-10">
            <div className="absolute -left-20 -top-20 h-64 w-64 rounded-full bg-white blur-3xl"></div>
            <div className="absolute -right-20 -bottom-20 h-64 w-64 rounded-full bg-white blur-3xl"></div>
          </div>

          <div className="relative z-10 mx-auto max-w-2xl">
            <h2 className="mb-6 text-4xl font-black tracking-tight sm:text-5xl">
                Ready to transform your visual architecture?
            </h2>
            <p className="mb-10 text-xl font-medium opacity-90 leading-relaxed">
              Join thousands of architects and designers building the future on FlowSpace.
              Start for free, no credit card required.
            </p>
            <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
              <Link href="/register">
                <Button size="lg" variant="secondary" className="h-14 px-8 text-lg font-black shadow-xl group">
                  Get Started Free
                  <ArrowRight className="ml-2 h-5 w-5 transition-transform group-hover:translate-x-1" />
                </Button>
              </Link>

            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
