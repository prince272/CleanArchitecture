
import { makeStyles } from '@mui/styles';
import { Container, LinearProgress } from '@mui/material';
import { useNProgress } from '@tanem/react-nprogress';
import React from 'react';

const useStyles = makeStyles({
    bar: ({ animationDuration }) => ({
        transitionDuration: `${animationDuration}ms`,
    }),
    container: ({ animationDuration, isFinished }) => ({
        opacity: isFinished ? 0 : 1,
        pointerEvents: 'none',
        transition: `opacity ${animationDuration}ms linear`,
    }),
})

const PageProgress = ({ animating }) => {
    const { animationDuration, isFinished, progress } = useNProgress({
        isAnimating: animating,
    })
    const classes = useStyles({ animationDuration, isFinished })

    return (
        <Container classes={{ root: classes.container }} disableGutters={true}>
            <LinearProgress
                classes={{ bar1Determinate: classes.bar }}
                value={progress * 100}
                variant="determinate"
            />
        </Container>
    )
}

export default PageProgress;
