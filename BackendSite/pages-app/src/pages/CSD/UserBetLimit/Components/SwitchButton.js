import React from "react"

export default function SwitchButton ({ checked, onChange, mainKey }) {
    let id = `switch-${mainKey}`;
    return (
      <div className="switch__container">
        <input
          id={id}
          className="switch switch--flat"
          type="checkbox"
          checked={checked}
          onChange={onChange}
        />
        <label htmlFor={id}></label>
      </div>
    );
  };