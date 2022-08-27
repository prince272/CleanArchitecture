import { createContext, useContext, useEffect, useState } from 'react';

const ClientContext = createContext();

const ClientProvider = ({ children, client }) => {
    const [user, setUser] = useState(client.store.subject.getValue());

    useEffect(() => {

        const subscription = client.store.subject
            .subscribe(value => {
                if (value !== user) setUser(value);
            });

        return () => {
            subscription.unsubscribe();
        };
    }, []);

    return (
        <ClientContext.Provider value={{ ...client, user }}>
            {children}
        </ClientContext.Provider>
    );
};

const ClientConsumer = ({ children }) => {
    return (
        <ClientContext.Consumer>
            {context => {
                if (context === undefined) {
                    throw new Error('ClientConsumer must be used within a ClientProvider')
                }
                return children(context)
            }}
        </ClientContext.Consumer>
    )
};

const useClient = () => {
    const context = useContext(ClientContext)
    if (context === undefined) {
        throw new Error('useClient must be used within a ClientProvider')
    }
    return context
};

export default ClientContext;
export { ClientProvider, ClientConsumer, useClient };