import { forwardRef, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { MuiTelInput } from 'mui-tel-input';
import { TextField } from '@mui/material';
import { isPhoneFormat } from '../utils';
import { getCountryCallingCode } from 'libphonenumber-js';

const PhoneTextField = forwardRef(({ value, onChange, defaultCountry = 'GH', phone = false, ...props }, forwardedRef) => {

  const Input = (phone || isPhoneFormat(value)) ? MuiTelInput : TextField;
  const inputRef = useRef();
  const [inputWasFocused, setWasFocused] = useState(false);

  const _onChnage = useCallback((event, info) => {
    onChange(event?.target ? event.target.value : event, info);
  }, [onChange]);

  const setNativeInputValue = (value) => {
    var nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
    nativeInputValueSetter.call(inputRef.current, value);
    inputRef.current.dispatchEvent(new Event('input', { bubbles: true }));
  };

  useMemo(() => {
    const focused = typeof document !== 'undefined' ? document.activeElement === inputRef.current : false
    setWasFocused(focused);
  }, [Input]);

  useEffect(() => {
    if (isPhoneFormat(inputRef.current.value) && !inputRef.current.value.startsWith('+')) {
      _onChnage(`+${getCountryCallingCode(defaultCountry)}${inputRef.current.value}`);
    }
    else {
      _onChnage(`${inputRef.current.value}`);
    }

    if (inputWasFocused) inputRef.current.focus();
  }, [Input]);

  return (<Input inputRef={inputRef} {...props} {...{ value, onChange: _onChnage, defaultCountry }} />);
});

PhoneTextField.displayName = 'PhoneTextField';

export default PhoneTextField;