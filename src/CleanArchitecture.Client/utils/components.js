import { forwardRef } from "react";

export const DynamicComponent = forwardRef(({ as, children, ...props }, ref) => {
    const Component = as;
    return (<Component ref={ref} {...props}>{children}</Component>);
});