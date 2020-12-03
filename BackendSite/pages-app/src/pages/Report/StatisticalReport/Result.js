import React from "react";

const turnOverValidBetDataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Today',
            headerStartwith: 'turnOverValidBetToday_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Yesterday',
            headerStartwith: 'turnOverValidBetYesterday_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Last 2 Days',
            headerStartwith: 'turnOverValidBetLastTwoDays_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'This Month',
            headerStartwith: 'turnOverValidBetThisMonth_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Last Month',
            headerStartwith: 'turnOverValidBetLastMonth_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        }
    ]
}

const turnOverValidBetDataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Category',
            dataColumn: "categoryName"
        },
        {
            columnTitle: 'Game',
            dataColumn: "gameName"
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBetToday_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetTodaySummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBetYesterday_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetYesterdaySummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBetLastTwoDays_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetLastTwoDaysSummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBetThisMonth_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetThisMonthSummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBetLastMonth_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetLastMonthSummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        }
    ]
}

const winLossDataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Today',
            headerStartwith: 'winLossToday_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Yesterday',
            headerStartwith: 'winLossYesterday_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Last 2 Days',
            headerStartwith: 'winLossLastTwoDays_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'This Month',
            headerStartwith: 'winLossThisMonth_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Last Month',
            headerStartwith: 'winLossLastMonth_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        }
    ]
}

const winLossDataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Category',
            dataColumn: "categoryName"
        },
        {
            columnTitle: 'Game',
            dataColumn: "gameName"
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLossToday_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossTodaySummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLossYesterday_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossYesterdaySummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLossLastTwoDays_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossLastTwoDaysSummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLossThisMonth_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossThisMonthSummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLossLastMonth_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        //hidden:props.HideShowBtn
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossLastMonthSummary",
            body:{
                attr:{
                    class:"text-right"
                },
            }
        }
    ]
}

const Panel = (props) => {
console.log(props);
    const turnOverValidBetGroupColumnDefsDefault = turnOverValidBetDataGroupColumnDefs(props);
    const turnOverValidBetColumnDefsDefault = turnOverValidBetDataColumnDefs(props);

    let turnOverValidBetGroupHeaderRows = [];
    let turnOverValidBetHeaderRows = [];

    let turnOverValidBetGroupColumnDefs = [];
    let turnOverValidBetColumnDefs = [];

    const winLossGroupColumnDefsDefault = winLossDataGroupColumnDefs(props);
    const winLossColumnDefsDefault = winLossDataColumnDefs(props);

    let winLossGroupHeaderRows = [];
    let winLossHeaderRows = [];

    let winLossGroupColumnDefs = [];
    let winLossColumnDefs = [];

    // if(props.HidenTitles.hiddenTitlesList.length > 0)
    // {

    //         columnDefsDefault.forEach(elem=>{
                
    //             let flag=true;
                
    //             props.HidenTitles.hiddenTitlesList.forEach(HiddenElem=>{
    //                 if(elem.dataColumn.startsWith(HiddenElem.Header))
    //                 {
    //                     flag=false;
    //                 }
    //             });

    //             if(flag)
    //             {
    //                 columnDefs.push(elem);
    //             }
    //         });
            
    //         groupColumnDefsDefault.forEach(elem=>{

    //                    let flag=true;

    //                     props.HidenTitles.hiddenTitlesList.forEach(HiddenElem=>{
    //                         if(elem.columnTitle===HiddenElem.Group)
    //                         {
    //                             flag=false;
    //                         }
    //                     });

    //                     if(flag)
    //                     {
    //                         groupColumnDefs.push(elem);
    //                     }
    //                     else
    //                     {
    //                         groupColumnDefs.push({
    //                             columnTitle: elem.columnTitle,
    //                             headerStartwith: elem.headerStartwith,
    //                             colSpan:  1,
    //                             btnText: 'show'
    //                         });

    //                     }
    //         });
        
    // }
    // else
    // {
        turnOverValidBetGroupColumnDefs = turnOverValidBetGroupColumnDefsDefault;
        turnOverValidBetColumnDefs = turnOverValidBetColumnDefsDefault;
        winLossGroupColumnDefs = winLossGroupColumnDefsDefault;
        winLossColumnDefs = winLossColumnDefsDefault;
    // }

    turnOverValidBetGroupColumnDefs.forEach((element, idx) => {
        turnOverValidBetGroupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''} colSpan={element.colSpan}
                bgcolor={element.bgcolor}>
                {element.columnTitle}
                {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button> */}
            </th>
        );
    });


    turnOverValidBetColumnDefs.forEach((element, idx) => {
        turnOverValidBetHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
                {element.columnTitle}
             </th>
        );
     });

     let turnOverValidBetBodyRows = props.Datas.map((data, idx) => {
        let row = turnOverValidBetColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    bgcolor={ data.gameId == 0 ? 'LightCyan' : ''}
                   >
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </td>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });


    let turnOverValidBetTotalRows = props.TotalResult.map((data, idx) => {
        let row = turnOverValidBetColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <th key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
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

    winLossGroupColumnDefs.forEach((element, idx) => {
        winLossGroupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''} colSpan={element.colSpan}
                bgcolor={element.bgcolor}>
                {element.columnTitle}
                {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button> */}
            </th>
        );
    });


    winLossColumnDefs.forEach((element, idx) => {
        winLossHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
                {element.columnTitle}
             </th>
        );
     });

     let winLossBodyRows = props.Datas.map((data, idx) => {
        let row = winLossColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    bgcolor={ data.gameId == 0 ? 'LightCyan' : ''}
                   >
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </td>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });


    let winLossTotalRows = props.TotalResult.map((data, idx) => {
        let row = winLossColumnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <th key={`td-${element.dataColumn}-${idx}`}
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
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
                {/* <button type="button" className="btn btn-approved" onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} disabled={props.ExportBtnDisabled}>Export Excel</button> */}
            </div>
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th colSpan="2">TurnOver(ValidBet)</th>
                        {turnOverValidBetGroupHeaderRows}
                    </tr>
                    <tr>
                        {turnOverValidBetHeaderRows}
                    </tr>
                </thead>
                <tbody>
                    {turnOverValidBetBodyRows}
                    {turnOverValidBetTotalRows}
                </tbody>
                <thead>
                    <tr>
                        <th colSpan="2">WinLoss</th>
                        {winLossGroupHeaderRows}
                    </tr>
                    <tr>
                        {winLossHeaderRows}
                    </tr>
                </thead>
                <tbody>
                    {winLossBodyRows}
                    {winLossTotalRows}
                </tbody>
            </table>
           
        </div>
        
        
    )
}

export default Panel;