import React, {Component} from 'react'

export const RadioBoxClassName = {
    'checkbox_checked': ' radio-checked',
    'is_disabled':' is-disabled',
    'default':''
}

export class RadioBox extends Component{
    handleClick=(e)=>{
        let _execute = !this.props.Disabled;

        if(_execute && this.props.HandleClick){
            let v = e.target.value;
            this.props.HandleClick(v, this.props.BindingValue);
        }
    }
    render() {
        let _className = this.props.ClassName ? this.props.ClassName : ''
        let disabled = this.props.Disabled ? RadioBoxClassName.is_disabled : ''
        return (
            <label className={`radio${_className}${disabled}`}>
                <input type="radio" name={this.props.RadioGroup} value={this.props.Value} checked={this.props.Checked} onChange={this.handleClick}/>
                <i className="icon icon-radio"></i>
                <span className="text">{this.props.DisplayText}</span>
            </label>
        );
    }
}