const runtimeUrl =
  (typeof window !== "undefined" && window.__ENV__ && window.__ENV__.VITE_API_URL) || "";
const baseUrl = (runtimeUrl || import.meta.env.VITE_API_URL || "https://localhost:7224").replace(/\/$/, "");

async function parseResponse(response) {
  const contentType = response.headers.get("content-type") || "";
  if (!contentType.includes("application/json")) {
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    return { success: true, message: "Operación exitosa", data: null };
  }

  const json = await response.json();

  if (!response.ok || json?.success === false) {
    throw new Error(json?.message || `HTTP ${response.status}`);
  }

  return json;
}

export async function apiRequest(path, options = {}) {
  const response = await fetch(`${baseUrl}/${path.replace(/^\//, "")}`, {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {}),
    },
    ...options,
  });

  return parseResponse(response);
}
