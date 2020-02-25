import React, {Component} from 'react'

export const CheckBoxClassName = {
    'checkbox_checked': ' checkbox-checked',
    'is_disabled':' is-disabled',
    'default':''
}

export class CheckBox extends Component{
    handleClick=(e)=>{
        let _execute = !this.props.Disabled;
        if(this.props.ConfirmAlert){
            let msg = this.props.Checked ? this.props.ConfirmAlert.CheckedAlertText : this.props.ConfirmAlert.UnCheckedAlertText;
            _execute = window.confirm(msg);
        }

        if(_execute && this.props.HandleClick){
            let v = e.target.value;
            this.props.HandleClick(v, this.props.BindingValue);
        }
    }
    render(){
        let displayText = this.props.DisplayText ? <span className="text">{this.props.DisplayText}</span> : null;
        let displayElem = this.props.DisplayElementText ? this.props.DisplayElementText : null
        let _className = this.props.ClassName ? this.props.ClassName : ''
        let disabled = this.props.Disabled ? CheckBoxClassName.is_disabled : ''
        let value = this.props.Value === undefined ? '' : this.props.Value;
        return(
            <label className={`checkbox${_className}${disabled}`}>
                <input type="checkbox" checked={this.props.Checked} value={value} onChange={this.handleClick}/>
                <i className="icon icon-checkbox"></i>
                {displayText}
                {displayElem}
            </label>
        )
    }
}

export const CheckList = (props)=>{
    let options = props.Options;
    let selected_datas = props.SelectedDatas;
    let elem = options && options.map(data=>{
        let mainKey = "add_user_"+data.DisplayText+data.Value;
        let checked = selected_datas.indexOf(data.Value) !== -1
        
        return(
            <CheckBox 
                key={mainKey}  
                Checked={checked}
                HandleClick={props.OnChange}
                BindingValue={props.BindingValue}
                DisplayText={data.DisplayText}
                Value={data.Value}
            />
        )
    });

    return (
        <div className="form__col">
            {elem}     
        </div>
    );    
}