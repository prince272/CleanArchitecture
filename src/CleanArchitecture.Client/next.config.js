/** @type {import('next').NextConfig} */
const nextConfig = {
    /* useEffect being called twice in Nextjs Typescript app [duplicate]
       source: https://stackoverflow.com/questions/71835580/useeffect-being-called-twice-in-nextjs-typescript-app */
  reactStrictMode: false,
  swcMinify: true,
}

module.exports = nextConfig
