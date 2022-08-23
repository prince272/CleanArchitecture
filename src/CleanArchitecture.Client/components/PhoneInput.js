import 'react-phone-number-input/style.css';
import ReactPhoneInput from 'react-phone-number-input';
import flags from 'react-phone-number-input/flags';
import { forwardRef } from 'react';
import { TextField } from '@mui/material';
import { isPhoneFormat } from '../utils';
import { CountrySelectWithIcon as CountrySelect }  from './CountrySelect';

const PhoneTextField = forwardRef(({ value, value2, onChange, onChange2, onKeyDown, ...props }, inputRef) => {
  return (
    <>
      <TextField {...props} value={isPhoneFormat(value2) ? value : value2} onChange={(e) => {
        const targetValue = e.target.value;

        if (isPhoneFormat(targetValue))
          onChange(targetValue);
        else
          onChange2(targetValue);

      }} inputRef={inputRef} />
    </>
  );
});

const PhoneInput = forwardRef(({ value, onChange, ...props }, ref) => {

  const value2 = value;

  return (
    <div>
      <ReactPhoneInput
      
        flags={flags}
        addInternationalOption={false}
        international={false}
        withCountryCallingCode={false}
        defaultCountry="GH"
        countrySelectComponent={CountrySelect}
        {...props}
        readOnly={props?.InputProps?.readOnly}
        {...{
          value: isPhoneFormat(value2) ? value : undefined,
          value2,
          onChange,
          onChange2: onChange
        }}
        inputComponent={PhoneTextField}
        ref={ref}
      />
      <style jsx>
        {`

           div > :global(.PhoneInput) {
            position: relative;
          }

          div > :global(.PhoneInput .PhoneInputCountry) {
            position: absolute;
            top: 0;
            right: 0;
            display: ${isPhoneFormat(value2, 'style') ? 'flex' : 'none'} !important;
            order: 1;
            margin-top: 21px;
            margin-left: -50px;
            margin-right: 16px;
          }
          div > :global(.PhoneInput .PhoneInputCountry .PhoneInputCountrySelect) {
            opacity: 0;
          }
          div > :global(.PhoneInput input) {
            padding-right: ${isPhoneFormat(value2, 'style') ? '60px' : '0px'};
          }
         `}
      </style>
    </div>
  )
});

export default PhoneInput;