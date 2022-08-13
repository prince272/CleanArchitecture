import { json } from "@remix-run/node";

const {
  Links,
  LiveReload,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  useLoaderData,
} = require("@remix-run/react");

export const meta = () => ({
  charset: "utf-8",
  title: "New Remix App",
  viewport: "width=device-width,initial-scale=1",
});

export async function loader() {
  return json({
    ENV: {
      SERVER_URL: process.env.SERVER_URL,
      CLIENT_URL: process.env.CLIENT_URL,
      ENV_MODE: process.env.NODE_ENV
    },
  });
}

export default function Root() {
  const data = useLoaderData();

  return (
    <html lang="en">
      <head>
        <Meta />

        <Links />
      </head>

      <body>
        <Outlet />

        <ScrollRestoration />

        <script
          dangerouslySetInnerHTML={{
            __html: `window.env = ${JSON.stringify(data.env)}`,
          }}
        />
        <Scripts />

        <LiveReload />
      </body>
    </html>
  );
}
