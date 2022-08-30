import { NextResponse } from 'next/server';
import { findContextualRoute } from './views/routes';

export function middleware(request) {

    if (findContextualRoute(request.url)) {
        return NextResponse.rewrite(new URL('/', request.url))
    }
    return NextResponse.next();
}

export const config = {
    matcher: ['/:path*'],
}