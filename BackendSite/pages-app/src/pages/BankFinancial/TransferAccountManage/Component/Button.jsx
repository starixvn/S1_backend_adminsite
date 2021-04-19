    import React from 'react';

export const BtnClass = {
    primary: 'newbtn newbtn-primary newbtn-w-min',
    secondary: 'newbtn newbtn-secondary newbtn-w-min'
}

const Button = ({_className, displayText, onClick, Disable})=>{
    let _disabled = Disable === true;
    return(
        <button className={`${_className}${_disabled ? ' btn-disabled' : ''}`} onClick={onClick} disabled={_disabled}>
            <span>{displayText}</span>
        </button>
    )
}

export default Button;