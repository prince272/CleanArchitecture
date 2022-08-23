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