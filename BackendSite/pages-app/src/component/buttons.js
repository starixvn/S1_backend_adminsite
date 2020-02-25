import React, {Component} from 'react'

export const buttonClass = {
    newbtnPrimary:'newbtn-primary',
    newbtnAdd:'newbtn-add',
    newbtnSecondary:'newbtn-secondary',
    newbtnTertiary:'newbtn-tertiary',
    newbtnHighLight:'newbtn-highlight',
    newbtnDark:'newbtn-dark'
}

export const buttonSize = {
    min:'newbtn-w-min',
    md:'newbtn-w-md',
    default:''
}

export const iconStyle = {
    refresh:'icon-refresh'
}

export const executingStatus = {
    'success':'success',
    'error':'error'
}

export const Button = ({ButtonSize=buttonSize.min, ToUpperCase=false, ...props})=>{
    let displayText = props.DisplayText && ToUpperCase ? props.DisplayText.toUpperCase() : props.DisplayText;
    return (
        <div className={`newbtn ${props.ButtonClass} ${ButtonSize}`} onClick={props.OnClick}>
            <span className='text'>{displayText}</span>
        </div>
    )
}

export const IconButton = (props)=>{
    return(
        <div className="newbtn newbtn-tertiary" onClick={props.OnClick}>
            <i className={`icon ${props.IconStyle}`}></i>
        </div>
    )    
}

export const BtnBar = ({SubmitTextToUpperCase=false, CancelTextToUpperCase=false, ...props})=>{
    let tips = null;
    if(props.ExecutingStatus!==null){
        tips = <BtnTips ExecutingStatus={props.ExecutingStatus} {...props}/>
    }

    if(props.ToUpperCase){
        SubmitTextToUpperCase = true;
        CancelTextToUpperCase = true;
    }

    return(
        <div className="newbtn-bar">
            {tips}
            <div className="newbtn-bar__group">
                <Button ToUpperCase={SubmitTextToUpperCase} DisplayText={props.SubmitDisplayText} ButtonClass={buttonClass.newbtnPrimary} OnClick={props.ClickSubmit}/>
                <Button ToUpperCase={CancelTextToUpperCase} DisplayText={props.CancelDisplayText} ButtonClass={buttonClass.newbtnSecondary} OnClick={props.Close}/>
            </div>
        </div>
    )    
}

export const BtnBarWithRightAndLeftGroup = (props)=>{
    return(
        <div className="newbtn-bar">
            <div className="newbtn-bar__group">
                <div className="newbtn-bar__col">
                    {props.LeftButtons}
                </div>
                <div className="newbtn-bar__col-right">
                    {props.RightButtons}
                </div>
            </div>
        </div>
    )    
}

const success_msg_stop_ms = 2500;
const failure_msg_stop_ms = 2500;
class BtnTips extends Component{
    intervalID = 0;
    componentWillUnmount(){
        clearInterval(this.intervalID);
    }
    render(){
        if(this.props.ExecutingStatus===executingStatus.success){
            this.intervalID = setInterval(this.props.Close, success_msg_stop_ms);
        }else{
            this.intervalID = setInterval(this.props.CloseTips, failure_msg_stop_ms)
        }        

        let content = null;
        switch(this.props.ExecutingStatus){
            case(executingStatus.success):
                content = (
                    <div className="status__item status__item--successful">
                        <i className="icon icon-done small"></i>
                        <span className="text">{this.props.SuccessAlert}</span>
                    </div>
                )
                break;
            case(executingStatus.error):
                    content= (
                        <div className="status__item status__item--fail">
                            <i className="icon icon-warning small"></i>{this.props.ErrorAlert}
                        </div>
                    )
                    break;
            default:
                content = <div></div>
                break;
        }


        return(
            <div className="newbtn-bar__status">
                {content}
            </div>
        )
    }
}