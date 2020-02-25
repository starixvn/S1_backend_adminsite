import React from 'react';

export const blockSize = {
    'md':'block-md'
}

export const blockBackgroundColor = {
    'gray':'block-gray'
}

export const blockAlign = {
    'block__col_left':'block__col-left',
    'default':'block__col'
}

export const Block = (props)=>{
    let _blockSize = props.BlockSize ? ' '+props.BlockSize : '';
    let _backgroundColor = props.BlockBackgroundColor ? ' '+props.BlockBackgroundColor:'';
    return(
        <div className={`block${_backgroundColor}${_blockSize}`}>
            <div className="block__head">
                <div className="block__row">
                    <div className="block__col">
                        <span>{props.Summary}</span>
                    </div>
                    {
                        props.AllowEdit ? (
                            <div className="block__col-right">
                                <div className="newbtn newbtn-lighticon" onClick={props.ClickEdit}>
                                    <i className="icon icon-mode_edit"></i>
                                </div>
                            </div>
                        ) : null
                    }                        
                </div>
            </div>
            <div className="block__main">
                {props.BlockContent}
            </div>
        </div>
    )
}

export const BlockSpace = (props)=>{
    let _blockSize = props.BlockSize ? ' '+props.BlockSize : '';
    let _backgroundColor = props.BlockBackgroundColor ? ' '+props.BlockBackgroundColor:'';
    return(
        <div className={`block${_backgroundColor}${_blockSize} block-spacing`}>
            <div className="block__row">
                {props.Content}
            </div>
        </div>
    )    
}

export const BlockCol = ({Align=blockAlign.default, ...props})=>{
    return(
        <div className={Align}>
            {props.Content}
        </div>
    )    
}