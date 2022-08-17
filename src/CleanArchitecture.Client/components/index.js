import { IconButton } from '@mui/material';
import * as Icons from '@mui/icons-material';
import PropTypes from 'prop-types';

const DialogCloseButton = (props) => {
    const { children, onClose, ...other } = props;
    return (
        <IconButton
            aria-label="close"
            onClick={onClose}
            sx={{
                position: 'absolute',
                right: 8,
                top: 8,
                color: (theme) => theme.palette.grey[500],
            }} {...other}
        >
            <Icons.Close />
        </IconButton>
    )
};

DialogCloseButton.propTypes = {
    onClose: PropTypes.func.isRequired,
};

export { DialogCloseButton };