import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import basicSsl from "@vitejs/plugin-basic-ssl";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "INVENTORY_");
  const proxy = {
    "/api": {
      target: process.env.INVENTORY_API_URL || env.INVENTORY_API_URL || "https://localhost:7080",
      changeOrigin: true,
      secure: false
    }
  };

  return {
    plugins: [react(), basicSsl()],
    server: { host: "localhost", port: 3000, strictPort: true, proxy },
    preview: { host: "localhost", port: 3000, strictPort: true, proxy }
  };
});
