import Document, { Html, Head, Main, NextScript } from 'next/document';

class MyDocument extends Document {

    static async getInitialProps(ctx) {
        const originalRenderPage = ctx.renderPage;

        // Run the React rendering logic synchronously
        ctx.renderPage = () =>
            originalRenderPage({
                // Useful for wrapping the whole react tree
                enhanceApp: (App) => App,
                // Useful for wrapping in a per-page basis
                enhanceComponent: (Component) => Component,
            });

        // Run the parent `getInitialProps`, it now includes the custom `renderPage`
        const initialProps = await Document.getInitialProps(ctx);
        const env = {
            SERVER_URL: process.env.SERVER_URL,
            CLIENT_URL: process.env.CLIENT_URL,
            ENV_MODE: process.env.NODE_ENV
        };

        return { ...initialProps, env };
    }

    render() {
        return (
            <Html>
                <Head>
                    <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" />
                </Head>
                <body>
                    <Main />

                    {/* We recommend keeping all our environment variables on the server and
                        exposing them to the browser through window.env */}
                    <script dangerouslySetInnerHTML={{ __html: `window.env = ${JSON.stringify(this.props.env)}` }} />
                    <NextScript />
                </body>
            </Html>
        )
    }
}

export default MyDocument