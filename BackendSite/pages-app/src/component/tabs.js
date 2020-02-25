import React from 'react';

const TabStatus = {
    default:'',
    selected:'is-selected'
}

// AllTabs = {
//     'tab1':{displayText:"displayText", tabKey:'key of tab1', hasFooter:false},
//     'tab2':{displayText:"displayText", tabKey:'key of tab2', hasFooter:false},
// }

const Tabs = (props)=>{
    let _tabs = props.AllTabs && Object.keys(props.AllTabs).map((tabKey)=>{
        let tab = props.AllTabs[tabKey];
        let tab_status = props.CurrentTab === tab.tabKey ? TabStatus.selected : TabStatus.default;
        return(
            <Tab key={`tab-${tab.tabKey}`} TabDisplayText={tab.displayText} TabStatus={tab_status} ClickTab={props.ClickTab.bind(null, tab.tabKey)}/>
        )
    })
    return(
        <div className="newtabs">
            {_tabs}
        </div>
    )
}

const Tab = (props)=>{
    return(
        <div className={`newtabs__tab ${props.TabStatus}`}>
            <div className="newbtn" onClick={props.ClickTab}>
                <span className="text">{props.TabDisplayText}</span>
            </div>
        </div>
    )    
}

export default Tabs;