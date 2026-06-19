"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiClient } from "@/lib/api";
import { useAuthStore } from "@/store/useAuthStore";
import { AuthResponse } from "@/types/auth";
import { isAxiosError } from "axios";

const registerSchema = z
  .object({
    displayName: z
      .string()
      .min(2, { message: "Name must be at least 2 characters" }),
    email: z.string().email({ message: "Invalid email address" }),
    password: z
      .string()
      .min(6, { message: "Password must be at least 6 characters" }),
    confirmPassword: z.string(),
    inviteCode: z.string().min(1, { message: "Invite code is required" }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

type RegisterFormValues = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormValues) => {
    try {
      setIsLoading(true);
      const response = await apiClient.post<AuthResponse>("/auth/register", {
        displayName: data.displayName,
        email: data.email,
        password: data.password,
        inviteCode: data.inviteCode,
      });

      const authData = response.data;
      setAuth(
        {
          id: authData.id,
          email: authData.email,
          displayName: authData.displayName,
        },
        authData.accessToken,
        authData.refreshToken,
      );

      toast.success("Account created successfully");
      router.push("/onboarding");
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        const data = error.response?.data as { detail?: string; errors?: Record<string, string[]> } | undefined;
        if (data?.detail?.toLowerCase().includes("invite")) {
          setError("inviteCode", { type: "server", message: "Invalid invite code" });
        } else if (data?.errors) {
          const serverErrors = data.errors;
          Object.keys(serverErrors).forEach((key) => {
            // Attempt to map server error keys (e.g., 'Email') to form fields ('email')
            const field = (key.charAt(0).toLowerCase() +
              key.slice(1)) as keyof RegisterFormValues;
            if (field in registerSchema.shape || field === "confirmPassword") {
              setError(field, { type: "server", message: serverErrors[key][0] });
            } else {
              toast.error(serverErrors[key][0]);
            }
          });
        } else {
          toast.error(data?.detail || "Registration failed");
        }
      } else {
        toast.error("Registration failed");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card className="w-full max-w-md">
      <CardHeader className="space-y-1">
        <CardTitle className="text-2xl font-bold">Create an account</CardTitle>
        <CardDescription>
          Enter your details below to create your FlowSpace account.
        </CardDescription>
      </CardHeader>
      <form onSubmit={handleSubmit(onSubmit)}>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="displayName">Full Name</Label>
            <Input
              id="displayName"
              placeholder="John Doe"
              {...register("displayName")}
            />
            {errors.displayName && (
              <p className="text-sm text-destructive">
                {errors.displayName.message}
              </p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="m@example.com"
              {...register("email")}
            />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input id="password" type="password" {...register("password")} />
            {errors.password && (
              <p className="text-sm text-destructive">
                {errors.password.message}
              </p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Confirm Password</Label>
            <Input
              id="confirmPassword"
              type="password"
              {...register("confirmPassword")}
            />
            {errors.confirmPassword && (
              <p className="text-sm text-destructive">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="inviteCode">Invite Code</Label>
            <Input
              id="inviteCode"
              type="password"
              placeholder="••••••••"
              {...register("inviteCode")}
            />
            {errors.inviteCode && (
              <p className="text-sm text-destructive">
                {errors.inviteCode.message}
              </p>
            )}
          </div>
        </CardContent>
        <CardFooter className="flex flex-col space-y-4">
          <Button className="w-full" type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Create Account
          </Button>
          <div className="text-center text-sm">
            Already have an account?{" "}
            <Link
              href="/login"
              className="underline underline-offset-4 hover:text-primary"
            >
              Sign in
            </Link>
          </div>
        </CardFooter>
      </form>
    </Card>
  );
}
