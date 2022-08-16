import PropTypes from 'prop-types';
import IconButton from '@mui/material/IconButton';
import CloseIcon from '@mui/icons-material/Close';

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
            <CloseIcon />
        </IconButton>
    )
};

DialogCloseButton.propTypes = {
    onClose: PropTypes.func.isRequired,
};

export { DialogCloseButton };