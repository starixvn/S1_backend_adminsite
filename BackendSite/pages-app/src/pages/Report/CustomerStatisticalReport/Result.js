import React from "react";
import Modal from "./components/Popout";
import CategoryModal from "./components/CategoryPopout";
import GameModal from "./components/GamePopout";
import DateDetailModal from "./components/DateDetailPopout";

const GroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'TurnOver(ValidBet)',
            headerStartwith: 'turnOverValidBet_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'WinLoss',
            headerStartwith: 'winLoss_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        }
    ]
}

const ColumnDefs = (props) => {
    return [
        {
            columnTitle: 'UserName',
            dataColumn: "userName",
            body:{
                format: userFormat,
                categoryBgcolor:"Bisque",
                style:{
                //     position: "sticky",
                // top: "36.8px",
                // left:"0",
                // height:"36.8px",
                // width:"150px",
                // zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                width:"150px",
                zIndex:"3"
            }
        },
        {
            columnTitle: 'Category',
            dataColumn: "categoryName",
            body:{
                categoryBgcolor:"#FFC9C9",
                bgcolor:"#e0fff0",
                style:{
                    // position:"sticky",
                    // left:"150",
                    // width:"100px",
                    // zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                width:"100px",
                zIndex:"3"
            }
        },
        {
            columnTitle: 'Game',
            dataColumn: "gameName",
            body:{
                categoryBgcolor:"LightCyan",
                bgcolor:"#e0fff0",
                style:{
                    // position:"sticky",
                    // zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"3"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBet_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    },
                    categoryBgcolor:"LightCyan",
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "36.8px",
                    height:"36.8px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetSummary",
            body:{
                attr:{
                    class:"text-right"
                },
                categoryBgcolor:"LightCyan",
                bgcolor:"LightGray",
                style:{}
            },
            style:{
                position: "sticky", 
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLoss_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    },
                    categoryBgcolor:"LightCyan",
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "36.8px",
                    height:"36.8px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossSummary",
            body:{
                attr:{
                    class:"text-right"
                },
                categoryBgcolor:"LightCyan",
                bgcolor:"LightGray",
                style:{}
            },
            style:{
                position: "sticky", 
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        }
    ]
}

const Panel = (props) => {

    let dataGroupColumnDefs = GroupColumnDefs(props);
    let dataColumnDefs = ColumnDefs(props);

    let dataGroupHeaderRows = [];
    let dataHeaderRows = [];

    let dataBodyRows=[];

    let dataFilterDatas=props.Datas;

    dataGroupColumnDefs.forEach((element, idx) => {
        dataGroupHeaderRows.push(
            <th key={`th-${element.columnTitle}-${idx}`}
                scope="col"
                colSpan={element.colSpan}
                style={element.style ? element.style : ''}>
                {element.columnTitle}
                {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button> */}
            </th>
        );
    });

    dataColumnDefs.forEach((element, idx) => {
        dataHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                style={element.style ? element.style : ''}>
                {element.columnTitle}
            </th>
        );
    });

    props.UserList.forEach((user,uidx)=>{
        let userFlag=true;
        user.categoryList.forEach((category,cidx)=>{
            let categoryFlag=true;
            category.gameList.forEach((game,gidx)=>{
            let getDataItem = dataFilterDatas.find(x=>x.custId===user.custId && x.categoryId===category.categoryId && x.gameId===game.gameId);
            
            if(getDataItem)
            {
                let idx = dataFilterDatas.indexOf(getDataItem);
                let row = dataColumnDefs.map((element, idx) => {
                        let subData = {};
                        if (element.body && element.body.subData) {
                            element.body.subData.forEach(s => { subData[s] = getDataItem[s] });
                        }
                        if(userFlag)
                        {
                            userFlag = false;

                                return (
                                    <td key={`td-${element.dataColumn}-${idx}`}
                                        className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                        rowSpan={user.gameCount + user.categoryCount + 1}
                                        //style={{position:"sticky",left:"0",zIndex:"1"}}
                                        bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                    >
                                        {element.body && element.body.format 
                                        ? <button className="btn btn-link" onClick={() => props.SetUserShowBtn(user.custId)}>{getDataItem[element.dataColumn]}</button>  
                                        : getDataItem[element.dataColumn]}
                                    </td>
                                );
                        }
                        else
                        {
                            if(element.dataColumn!=="userName")
                            {
                                if(element.dataColumn==="categoryName")
                                {
                                    if(categoryFlag)
                                    {
                                    categoryFlag=false;
                                        return (
                                            <td key={`td-${element.dataColumn}-${idx}`}
                                                className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                                rowSpan={category.gameList.length + 1}
                                                //style={{position:"sticky",left:"0",zIndex:"1"}}
                                                bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                            >
                                                {/* {element.body && element.body.format ? element.body.format(getDataItem[element.dataColumn], subData) : getDataItem[element.dataColumn]} */}
                                                <button className="btn btn-link" onClick={() => props.SetUserCategoryShowBtn(user.custId,category.categoryId)}>{getDataItem[element.dataColumn]}</button>
                                                {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.TurnOverValidBetHideShowBtn(getDataItem['categoryId'])}>hide</button> */}
                                            </td>
                                        );
                                    }
                                }
                                else if (element.dataColumn==="gameName")
                                {
                                    return (
                                        <td key={`td-${element.dataColumn}-${idx}`}
                                            className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                            style={element.body && element.body.style ? element.body.style : ''}
                                            bgcolor={element.body && element.body.bgcolor ? element.body.bgcolor : ''}
                                        >
                                            {/* {element.body && element.body.format ? element.body.format(getDataItem[element.dataColumn], subData) : getDataItem[element.dataColumn]} */}
                                            <button className="btn btn-link" onClick={() => props.SetUserGameShowBtn(user.custId,category.categoryId,game.gameId,props.SetUserUserShowBtn)}>{getDataItem[element.dataColumn]}</button>
                                        </td>
                                    );
                                }
                                else
                                {
                                    return (
                                        <td key={`td-${element.dataColumn}-${idx}`}
                                            className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                            style={element.body && element.body.style ? element.body.style : ''}
                                            bgcolor={element.body && element.body.bgcolor ? element.body.bgcolor : ''}
                                        >
                                            {element.body && element.body.format ? element.body.format(getDataItem[element.dataColumn], subData) : getDataItem[element.dataColumn]}
                                        </td>
                                    );
                                }
                            }
                        }
                    });
    
                dataBodyRows.push(
                    <tr key={`result-body-data-${idx}`}>
                        {row}
                    </tr>
                );
            }
        });
            let getCategoryTotalItem = props.UserCategoryTotalResult.find(x=>x.custId===user.custId && x.categoryId===category.categoryId);

        if(getCategoryTotalItem)
        {
            let row = dataColumnDefs.map((element, idx) => {
                let subData = {};
                if (element.body && element.body.subData) {
                    element.body.subData.forEach(s => { subData[s] = getCategoryTotalItem[s] });
                }

                if(element.dataColumn!=="userName" && element.dataColumn!=="categoryName")
                {
                return (
                        <td key={`td-${element.dataColumn}-${idx}`}
                            className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                            bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                            style={element.body && element.body.style ? element.body.style : ''}
                        >
                        {element.body && element.body.format ? element.body.format(getCategoryTotalItem[element.dataColumn], subData) : getCategoryTotalItem[element.dataColumn]}
                    </td>
                 );
                }
                
            });
    
            dataBodyRows.push(
                <tr key={`result-body-user-${uidx}-category-${cidx}`}>
                    {row}
                 </tr>
            );
         }
       });

       let getTotalItem = props.UserTotalResult.find(x=>x.custId===user.custId);

       if(getTotalItem)
       {
           let row = dataColumnDefs.map((element, idx) => {
               let subData = {};
               if (element.body && element.body.subData) {
                   element.body.subData.forEach(s => { subData[s] = getTotalItem[s] });
               }

               if(element.dataColumn!=="userName")
               {
               return (
                       <td key={`td-${element.dataColumn}-${idx}`}
                           className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                           bgcolor={'#E8C9FF'}
                           style={element.body && element.body.style ? element.body.style : ''}
                       >
                       {element.body && element.body.format ? element.body.format(getTotalItem[element.dataColumn], subData) : getTotalItem[element.dataColumn]}
                   </td>
                );
               }
               
           });
   
           dataBodyRows.push(
               <tr key={`result-body-user-${uidx}`}>
                   {row}
                </tr>
           );
        }

    });
    
    let dataTotalRows = props.TotalResult.map((data, idx) => {
        let row = dataColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <th key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    style={element.body && element.body.style ? element.body.style : ''}
                    bgcolor="LemonChiffon">
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </th>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    return (
        <div className="table">
            
            <div className="table-options">
                <button type="button" className="btn btn-approved" 
                        onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} 
                        disabled={props.ExportBtnDisabled}>Export Excel</button>
            </div>

            <table className="table table-striped">
                <thead>
                    <tr>
                    <th colSpan="3" style={{ position: "sticky", top: "0",height:"36.8px",zIndex:"3",background:"#FFFF45",color:"#F00000"}}>Customer Summary By Category And Game</th>
                        {dataGroupHeaderRows}
                    </tr>
                    <tr>
                        {dataHeaderRows}
                    </tr>
                </thead>
                <tbody>
                    {dataBodyRows}
                    {dataTotalRows}
                </tbody>
            </table>

            <Modal handleClose={e => props.CloseBtn()}
                   show={props.UserDataShowStatus}
                   userdata={props.UserDataShowLists}
                   dateDatas={props.DatasByDate}
                   userTotalDatas={props.UserTotalResult}
                   userCategoryDataShow={props.SetUserCategoryShowBtn}
                   userGameDataShow={props.SetUserGameShowBtn}
                   userDateDataShow={props.SetUserDateShowBtn}/>

            <CategoryModal handleClose={e => props.CloseBtn()}
                           handleBack={props.SetUserShowBtn}
                           show={props.UserCategoryDataShowStatus}
                           useroriginaldata={props.UserList}
                           userdata={props.UserCategoryDataShowLists}
                           dateDatas={props.DatasByDate}
                           userCategoryTotalDatas={props.UserCategoryTotalResult}
                           userGameDataShow={props.SetUserGameShowBtn}
                           userDateDataShow={props.SetUserDateShowBtn}/>

            <GameModal handleClose={e => props.CloseBtn()}
                       handleBack={props.SetUserCategoryShowBtn}
                           show={props.UserGameDataShowStatus}
                           useroriginaldata={props.UserList}
                           userdata={props.UserGameDataShowLists}
                           dateDatas={props.DatasByDate}
                           userGameTotalDatas={props.Datas}
                           userDateShowBtn={props.SetUserDateShowBtn}
                           userCategoryDataShow={props.SetUserCategoryShowBtn}/>

            <DateDetailModal handleClose={e => props.CloseBtn()}
                             handleBack={props.SetUserGameShowBtn}
                           show={props.UserDateDataShowStatus}
                           useroriginaldata={props.UserList}
                           userdata={props.UserDateDataShowLists}
                           dateDetailDatas={props.DateDetailDatas}
                           dateDetailTotalDatas={props.DateDetailTotalDatas}/>
        </div>  
        
    )
}

const userFormat = (userName) => {
    return userName;
}

export default Panel;