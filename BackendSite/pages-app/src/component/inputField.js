import React, {Component} from 'react'

export const textFieldStatus = {
    'readonly':'is-readonly',
    'edited':'is-edited'
}

export class InputBox extends Component{
    CheckValue=(v)=>{
        let result = { Checked:true, AlertMsg:'' };
        const alertMsg = (alertMsg)=> { return alertMsg ? alertMsg : 'Wrong format.'};
        if(v && this.props.Rule){
            let rule = this.props.Rule;
            let length = this.props.PrefixText ? this.props.PrefixText.length + v.length : v.length;
            if(rule.MaxLength && rule.MaxLength.Value < length){
                result.Checked = false;
                result.AlertMsg = alertMsg(rule.MaxLength.AlertMsg)
            }
            else if(rule.MinLength && rule.MinLength.Value > length){
                result.Checked = false;
                result.AlertMsg = alertMsg(rule.MinLength.AlertMsg)
            }
            else if(rule.Regex && !v.match(rule.Regex.Value)){
                result.Checked = false;
                result.AlertMsg = alertMsg(rule.Regex.AlertMsg)
            }
        }
        return result;
    }
    OnBlur=(e)=>{
        if(this.props.OnBlur){
            if(this.props.DataColumn){
                this.props.OnBlur(e.target.value, this.props.DataColumn)
            }else{
                this.props.OnBlur(e.target.value)
            }

            var checkResult = this.CheckValue(e.target.value);
            if(!checkResult.Checked){
                alert(checkResult.AlertMsg)
            }
        }
    }
    OnChange=(e)=>{
        if(this.props.OnChange){
            if(this.props.DataColumn){
                this.props.OnChange(e.target.value, this.props.DataColumn)
            }else{
                this.props.OnChange(e.target.value)
            }
        }
    }
    render(){
        let isEdited = this.props.IsEdited ? ' '+textFieldStatus.edited : '';
        let isReadonly = this.props.ReadOnly ? ' '+textFieldStatus.readonly : '';
        let mainKey = this.props.MainKey ? this.props.MainKey : '';

        let tipsElement = this.props.TipsElement
        let key = this.props.Key ? this.props.Key : `ipt-${mainKey}-${new Date()}-${this.props.Value}`;
        if(this.props.ControlledByParent===true){
            return(
                <div className={`textfield${isEdited}${isReadonly}`} key={key}>
                    {this.props.PrefixDisplayText}
                    <input type="text" className="textfield__input" onBlur={this.OnBlur} value={this.props.Value} onChange={this.OnChange}/>
                    {tipsElement}
                </div>
            )
        }else{
            return(
                <div className={`textfield${isEdited}${isReadonly}`} key={key}>
                    {this.props.PrefixDisplayText}
                    <input type="text" className="textfield__input" onBlur={this.OnBlur} defaultValue={this.props.Value} onChange={this.OnChange}/>
                    {tipsElement}
                </div>
            )
        }
    }
}

InputBox.defaultProps = {
    PrefixDisplayText:null,
    TipsElement:null
}


export const InputBoxTips = ({TipsIcon=null, ...props})=>{
    let isShow = props.IsShow ? ' is-show' : null
    return(
        <div className={`textfield__tips${isShow}`}>
            {TipsIcon}
            {props.TipsDisplayText}
        </div>
    )
}

export class TextArea extends Component{
    OnBlur=(e)=>{
        if(this.props.DataColumn){
            this.props.OnBlur(e.target.value, this.props.DataColumn)
        }else{
            this.props.OnBlur(e.target.value)
        }
    }
    render(){
        return(
            <div className="textfield">
                <textarea className="textfield__textarea" onBlur={this.OnBlur} defaultValue={this.props.Value}/>
            </div>
        )
    }
}