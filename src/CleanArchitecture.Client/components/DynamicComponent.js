import { forwardRef } from "react";

const DynamicComponent = forwardRef(({ as, children, ...props }, ref) => {
    const Component = as;
    return (<Component ref={ref} {...props}>{children}</Component>);
});

DynamicComponent.displayName = 'DynamicComponent';

export { DynamicComponent };