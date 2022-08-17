import { useCallback, useEffect, useRef } from 'react';
import { useRouter } from 'next/router';
import QueryString from 'qs';

export const RETURN_PATH_QUERY_PARAM = '_UCR_RETURN_PATH';

/**
 * During contextual routing browser URL will be controlled by Next Router's "as" prop
 * while the page actually rendered is defined by Next Router's "href" prop.
 *
 * During contextual navigation Next Router's behaves as follows:
 * router.asPath:   /item/3               reflects current URL and updates at each page change
 * router.pathname: /search/[terms]       stay the same as long as initial page doesn't change
 * router.query:    {"terms": "foo-bar"}  same as above
 */
export function useContextualRouting() {
  const router = useRouter();
  const returnPathQueryParam = router.query[RETURN_PATH_QUERY_PARAM];
  const watchedQuery = Object.assign({}, router.query);
  delete watchedQuery[RETURN_PATH_QUERY_PARAM];

  /*
   * After a page refresh there is no RETURN_PATH_QUERY_PARAM in router.query
   * RETURN_PATH_QUERY_PARAM is only available in those history entries where
   * contextual navigation is enabled (or WAS enabled in case history.back() is triggered)
   */
  const returnPath = returnPathQueryParam ?? '/';
  const contextualPath = returnPathQueryParam ? router.asPath : null;

  // @NOTE JSON.stringify might be replaced with any hashing solution
  const queryHash = JSON.stringify(watchedQuery);
  const constructContextualPath = useCallback(
    (url) => {
      url = require('url').format(url);
      const extraParams = QueryString.parse(url.substring(url.indexOf('?') + 1));

      const linkProps = {
        as: url,
        href: router.pathname + '?' + QueryString.stringify(
          Object.assign({}, extraParams, { [RETURN_PATH_QUERY_PARAM]: returnPath })
        )
      };

      return linkProps;
    },
    [queryHash, returnPath]
  );

  return { returnPath, contextualPath, constructContextualPath };
}

// react hook for waiting state update (useAsyncState)
// source: https://dev.to/adelchms96/react-hook-for-waiting-state-update-useasyncstate-147g
export function withAsyncState([state, setState]) {
  const setter = x =>
    new Promise(resolve => {
      setState(x);
      resolve(x);
    });
  return [state, setter];
}