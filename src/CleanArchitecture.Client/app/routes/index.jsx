import { json } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";
import client from "client";

export async function loader() {
 const response = await client.signin({ username: "princeowusu.272@gmail.com", password: "Owusu#15799" });
 return json(response.data);
}

export default function Index() {
  const profile = useLoaderData();

  return (
    <div style={{ fontFamily: "system-ui, sans-serif", lineHeight: "1.4" }}>
      <h1>Welcome to Remix</h1>
      {JSON.stringify(profile)}
      <ul>
        <li>
          <a
            target="_blank"
            href="https://remix.run/tutorials/blog"
            rel="noreferrer"
          >
            15m Quickstart Blog Tutorial
          </a>
        </li>

        <li>
          <a
            target="_blank"
            href="https://remix.run/tutorials/jokes"
            rel="noreferrer"
          >
            Deep Dive Jokes App Tutorial
          </a>
        </li>

        <li>
          <a target="_blank" href="https://remix.run/docs" rel="noreferrer">
            Remix Docs
          </a>
        </li>
      </ul>
    </div>
  );
}
