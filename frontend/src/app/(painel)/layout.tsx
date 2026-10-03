"use client";

import { AdminShell } from "@/components/admin-shell";
import type { ReactNode } from "react";

export default function PainelLayout({ children }: { children: ReactNode }) {
  return <AdminShell>{children}</AdminShell>;
}
