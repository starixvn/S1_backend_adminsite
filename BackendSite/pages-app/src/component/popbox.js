import React from 'react'

export const popboxWrapperClassNames = {
    'popup_wrap__tabcontent':'popup-wrap--tabcontent'
}

export const popboxSize = {
    'min':'popup-min',
    'sm':'popup-sm',
    'default':''
}

export const Popbox = (props)=>{
    const popboxWrapperClassName = props.PopboxWrapperClassName ? " " + props.PopboxWrapperClassName : ""
    const popboxSize = props.PopboxSize ? " "+props.PopboxSize : ""
    const title = props.Title ? props.Title : ""
    const Content = props.Content;
    return(
        <div className={`popup-wrap${popboxWrapperClassName}${props.IsOpen ? " is-show" : ""}`}>
            <div className={`popup${popboxSize}${props.IsOpen ? " is-open" : ""}`}>
                <div className="popup__head">
                    <span className="text">{title}</span>
                    {
                        props.RemoveCloseBtn === true ? (
                            null
                        ) : (
                            <div className="newbtn">
                                <i className="icon icon-close" onClick={props.ClickClose}></i>
                            </div>
                        )
                    }
                </div>
                {Content}
            </div>
        </div>
    )    
}