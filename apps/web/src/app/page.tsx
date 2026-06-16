import { Navbar } from "@/components/marketing/Navbar";
import { Hero } from "@/components/marketing/Hero";
import { Features } from "@/components/marketing/Features";
import { Cta } from "@/components/marketing/Cta";

export default function LandingPage() {
  return (
    <div className="flex min-h-screen flex-col bg-background selection:bg-primary/10">
      <Navbar />
      <main className="flex-1">
        <Hero />
        <Features />
        {/* Template Gallery and more can go here */}
        <Cta />
      </main>
      <footer className="border-t bg-muted/20 py-12">
        <div className="container mx-auto px-4">
          <div className="flex flex-col items-center justify-between gap-6 md:flex-row">
            <div className="flex items-center gap-2 font-black text-xl tracking-tighter">
              FlowSpace
            </div>
            <p className="text-sm text-muted-foreground font-medium">
              © 2026 FlowSpace Engine. Built with Gemini 1.5.
            </p>
            <div className="flex gap-6">
              <a href="#" className="text-sm font-medium text-muted-foreground hover:text-primary">Twitter</a>
              <a href="#" className="text-sm font-medium text-muted-foreground hover:text-primary">GitHub</a>
              <a href="#" className="text-sm font-medium text-muted-foreground hover:text-primary">Terms</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
