import { NextResponse } from 'next/server';
import { findContextualRoute, PAGE_PATH_QUERY_PARAM } from './views/routes';

export function middleware(request) {

    if (findContextualRoute(request.nextUrl.toString())) { 
        const pagePath = request.nextUrl.searchParams.get(PAGE_PATH_QUERY_PARAM) || '/';
        return NextResponse.rewrite(new URL(pagePath, request.nextUrl));
    }  

    return NextResponse.next();
}