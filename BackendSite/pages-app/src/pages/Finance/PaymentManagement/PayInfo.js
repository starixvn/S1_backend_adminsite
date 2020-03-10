import React from "react";
import {OuterPanel, SwitchBtn}  from './components';
import {ChannelInfoHeader, ChannelInfoContent} from './ChannelInfo';

export const PayInfoHeader = (props)=>{
    return(
        <span style={{'color':'white'}}>{props.Name}</span>
    );
}

export const PayInfoContent = (props)=>{
    let channelList = props.ChannelList.map((channel, idx)=>{
        return(
            <OuterPanel
                key={`channelList-${idx}`}
                Id={`${props.MainId}-channel-${idx}`}
                HeaderContent={
                    <ChannelInfoHeader Name={channel.channelName}/>
                }
                Content={
                    <ChannelInfoContent 
                        {...channel} 
                        idxArr={[...props.idxArr, idx]} 
                        ClickEdit={props.ClickEdit}
                        ClickCancel={props.ClickCancel}
                        ClickSave={props.ClickSave}
                        EditField={props.EditField}
                    />
                }
            />
        );
    });

    return(
        <div className="table">
            <div className="table-options">
                <SwitchBtn Title="Status" checked={props.switchBtn.Status===1} onChange={props.switchBtn.onChange} SwitchBtnKey={props.switchBtn.SwitchBtnKey}/>                
            </div>
            {channelList}
        </div>
    )
}