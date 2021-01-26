import React from "react";

const turnOverValidBetDataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Today',
            headerStartwith: 'turnOverValidBetToday_',
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
            columnTitle: 'Yesterday',
            headerStartwith: 'turnOverValidBetYesterday_',
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
            columnTitle: 'Last 2 Days',
            headerStartwith: 'turnOverValidBetLastTwoDays_',
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
            columnTitle: 'This Month',
            headerStartwith: 'turnOverValidBetThisMonth_',
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
            columnTitle: 'Last Month',
            headerStartwith: 'turnOverValidBetLastMonth_',
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

const turnOverValidBetDataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Category',
            dataColumn: "categoryName",
            body:{
                categoryBgcolor:"Bisque",
                style:{
                    position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
                zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
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
                    position:"sticky",
                    left:"88.73px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left: "88.73px",
                height:"36.8px",
                zIndex:"3"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBetToday_${curr.id}`,
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
            dataColumn: "turnOverValidBetTodaySummary",
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
                dataColumn: `turnOverValidBetYesterday_${curr.id}`,
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
            dataColumn: "turnOverValidBetYesterdaySummary",
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
                dataColumn: `turnOverValidBetLastTwoDays_${curr.id}`,
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
            dataColumn: "turnOverValidBetLastTwoDaysSummary",
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
                dataColumn: `turnOverValidBetThisMonth_${curr.id}`,
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
            dataColumn: "turnOverValidBetThisMonthSummary",
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
                dataColumn: `turnOverValidBetLastMonth_${curr.id}`,
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
            dataColumn: "turnOverValidBetLastMonthSummary",
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

const winLossDataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Today',
            headerStartwith: 'winLossToday_',
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
            columnTitle: 'Yesterday',
            headerStartwith: 'winLossYesterday_',
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
            columnTitle: 'Last 2 Days',
            headerStartwith: 'winLossLastTwoDays_',
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
            columnTitle: 'This Month',
            headerStartwith: 'winLossThisMonth_',
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
            columnTitle: 'Last Month',
            headerStartwith: 'winLossLastMonth_',
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

const winLossDataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Category',
            dataColumn: "categoryName",
            body:{
                categoryBgcolor:"Bisque",
                style:{
                    position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
                zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left:"0",
                height:"36.8px",
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
                    position:"sticky",
                    left:"88.73px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "36.8px",
                left: "88.73px",
                height:"36.8px",
                zIndex:"3"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLossToday_${curr.id}`,
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
            dataColumn: "winLossTodaySummary",
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
                dataColumn: `winLossYesterday_${curr.id}`,
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
            dataColumn: "winLossYesterdaySummary",
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
                dataColumn: `winLossLastTwoDays_${curr.id}`,
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
            dataColumn: "winLossLastTwoDaysSummary",
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
                dataColumn: `winLossThisMonth_${curr.id}`,
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
            dataColumn: "winLossThisMonthSummary",
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
                dataColumn: `winLossLastMonth_${curr.id}`,
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
            dataColumn: "winLossLastMonthSummary",
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
    const turnOverValidBetGroupColumnDefsDefault = turnOverValidBetDataGroupColumnDefs(props);
    const turnOverValidBetColumnDefsDefault = turnOverValidBetDataColumnDefs(props);

    let turnOverValidBetGroupHeaderRows = [];
    let turnOverValidBetHeaderRows = [];

    let turnOverValidBetGroupColumnDefs = [];
    let turnOverValidBetColumnDefs = [];

    let turnOverValidBetBodyRows=[];

    const winLossGroupColumnDefsDefault = winLossDataGroupColumnDefs(props);
    const winLossColumnDefsDefault = winLossDataColumnDefs(props);

    let winLossGroupHeaderRows = [];
    let winLossHeaderRows = [];

    let winLossGroupColumnDefs = [];
    let winLossColumnDefs = [];

    let winLossBodyRows=[];

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

    let turnOverValidBetFilterDatas=[];

    if(props.HidenTitles.turnOverValidBetHiddenCategoryList.length > 0)
    {      
        props.Datas.forEach(data => {

            let flag=true;
            props.HidenTitles.turnOverValidBetHiddenCategoryList.forEach(category=>{

                if(data.categoryId===category.CategoryId)
                {
                    flag=false;
                }

            });

            if(flag)
            {
                turnOverValidBetFilterDatas.push(data);
            }
        });
    }
    else
    {
        turnOverValidBetFilterDatas=props.Datas;
    }

    turnOverValidBetGroupColumnDefs.forEach((element, idx) => {
        turnOverValidBetGroupHeaderRows.push(
            <th key={`th-${element.columnTitle}-${idx}`}
                scope="col"
                colSpan={element.colSpan}
                style={element.style ? element.style : ''}>
                {element.columnTitle}
                {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button> */}
            </th>
        );
    });

    turnOverValidBetColumnDefs.forEach((element, idx) => {
        turnOverValidBetHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                style={element.style ? element.style : ''}>
                {element.columnTitle}
            </th>
        );
     });

    props.CategoryGameList.forEach((category,cidx)=>{
        let flag=true; 
        category.gameList.forEach((game)=>{
            
            let getDataItem = turnOverValidBetFilterDatas.find(x=>x.categoryId===category.categoryId && x.gameId===game.gameId);
            
            if(getDataItem)
            {
                let idx = turnOverValidBetFilterDatas.indexOf(getDataItem);
                let row = turnOverValidBetColumnDefs.map((element, idx) => {
                        let subData = {};
                        if (element.body && element.body.subData) {
                            element.body.subData.forEach(s => { subData[s] = getDataItem[s] });
                        }
                        if(flag)
                        {
                            flag = false;
                                return (
                                    <td key={`td-${element.dataColumn}-${idx}`}
                                        className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                        rowSpan={category.gameList.length + 1}
                                        style={{position:"sticky",left:"0",zIndex:"1"}}
                                        bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                    >
                                        {element.body && element.body.format ? element.body.format(getDataItem[element.dataColumn], subData) : getDataItem[element.dataColumn]}
                                        {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.TurnOverValidBetHideShowBtn(getDataItem['categoryId'])}>hide</button> */}
                                    </td>
                                );
                        }
                        else
                        {
                            if(element.dataColumn!=="categoryName")
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
                    });
    
                turnOverValidBetBodyRows.push(
                    <tr key={`result-body-data-${idx}`}>
                        {row}
                    </tr>
                );
            }
        });

        let geCategoryItem = props.CategoryTotalResult.find(x=>x.categoryId===category.categoryId);
            
        if(geCategoryItem)
        {
            let flag=true;

            props.HidenTitles.turnOverValidBetHiddenCategoryList.forEach(id=>{

                if(category.categoryId===id.CategoryId)
                {
                    flag=false;
                }
            });

            let row = turnOverValidBetColumnDefs.map((element, idx) => {
                let subData = {};
                if (element.body && element.body.subData) {
                    element.body.subData.forEach(s => { subData[s] = geCategoryItem[s] });
                }
                    
                if(flag){
                    if(element.dataColumn!=="categoryName"){
                        return (
                            <td key={`td-${element.dataColumn}-${idx}`}
                                className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                style={element.body && element.body.style ? element.body.style : ''}
                            >
                                {element.body && element.body.format ? element.body.format(geCategoryItem[element.dataColumn], subData) : geCategoryItem[element.dataColumn]}
                            </td>
                        );
                    }
                }
                else{
                        return (
                            <td key={`td-${element.dataColumn}-${idx}`}
                                className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                style={element.body && element.body.style ? element.body.style : ''}
                            >
                                {element.body && element.body.format ? element.body.format(geCategoryItem[element.dataColumn], subData) : geCategoryItem[element.dataColumn]}
                                {element.dataColumn==="categoryName" ? <button type="button" className="btn btn-info pull-right" onClick={() => props.TurnOverValidBetHideShowBtn(geCategoryItem['categoryId'])}>show</button> : ''}
                            </td>
                        );
                }
            });
    
            turnOverValidBetBodyRows.push(
                <tr key={`result-body-category-${cidx}`}>
                    {row}
                 </tr>
            );
        }

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


    winLossGroupColumnDefs.forEach((element, idx) => {
        winLossGroupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                colSpan={element.colSpan}
                style={element.style ? element.style : ''}bgcolor={element.bgcolor}>
                {element.columnTitle}
                {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button> */}
            </th>
        );
    });

    winLossColumnDefs.forEach((element, idx) => {
        winLossHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                style={element.style ? element.style : ''}>
                {element.columnTitle}
            </th>
        );
     });

    let winLossFilterDatas=[];

    if(props.HidenTitles.winLossHiddenCategoryList.length > 0)
    {      
        props.Datas.forEach(data => {

            let flag=true;
            props.HidenTitles.winLossHiddenCategoryList.forEach(category=>{

                if(data.categoryId===category.CategoryId)
                {
                    flag=false;
                }

            });

            if(flag)
            {
                winLossFilterDatas.push(data);
            }
        });
    }
    else
    {
        winLossFilterDatas=props.Datas;
    }

    props.CategoryGameList.forEach((category,cidx)=>{
        let flag=true; 
        category.gameList.forEach((game)=>{
            
            let getDataItem = winLossFilterDatas.find(x=>x.categoryId===category.categoryId && x.gameId===game.gameId);
            
            if(getDataItem)
            {
                let idx = winLossFilterDatas.indexOf(getDataItem);
                let row = winLossColumnDefs.map((element, idx) => {
                    let subData = {};
                    if (element.body && element.body.subData) {
                        element.body.subData.forEach(s => { subData[s] = getDataItem[s] });
                    }
                    if(flag)
                    {
                        flag = false;
                            return (
                                <td key={`td-${element.dataColumn}-${idx}`}
                                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                    rowSpan={category.gameList.length + 1}
                                    style={{position:"sticky",left:"0",zIndex:"1"}}
                                    bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                >
                                    {element.body && element.body.format ? element.body.format(getDataItem[element.dataColumn], subData) : getDataItem[element.dataColumn]}
                                    {/* <button type="button" className="btn btn-info pull-right" onClick={() => props.TurnOverValidBetHideShowBtn(getDataItem['categoryId'])}>hide</button> */}
                                </td>
                            );
                    }
                    else
                    {
                        if(element.dataColumn!=="categoryName")
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
                });
    
                winLossBodyRows.push(
                    <tr key={`result-body-data-${idx}`}>
                        {row}
                    </tr>
                );
            }
        });

        let geCategoryItem = props.CategoryTotalResult.find(x=>x.categoryId===category.categoryId);
            
        if(geCategoryItem)
        {
            let flag=true;

            props.HidenTitles.winLossHiddenCategoryList.forEach(id=>{

                if(category.categoryId===id.CategoryId)
                {
                    flag=false;
                }
            });

            let row = winLossColumnDefs.map((element, idx) => {
                let subData = {};
                if (element.body && element.body.subData) {
                    element.body.subData.forEach(s => { subData[s] = geCategoryItem[s] });
                }
                    
                if(flag){
                    if(element.dataColumn!=="categoryName"){
                        return (
                            <td key={`td-${element.dataColumn}-${idx}`}
                                className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                style={element.body && element.body.style ? element.body.style : ''}
                            >
                                {element.body && element.body.format ? element.body.format(geCategoryItem[element.dataColumn], subData) : geCategoryItem[element.dataColumn]}
                            </td>
                        );
                    }
                }
                else{
                        return (
                            <td key={`td-${element.dataColumn}-${idx}`}
                                className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                                bgcolor={element.body && element.body.categoryBgcolor ? element.body.categoryBgcolor : ''}
                                style={element.body && element.body.style ? element.body.style : ''}
                            >
                                {element.body && element.body.format ? element.body.format(geCategoryItem[element.dataColumn], subData) : geCategoryItem[element.dataColumn]}
                                {element.dataColumn==="categoryName" ? <button type="button" className="btn btn-info pull-right" onClick={() => props.TurnOverValidBetHideShowBtn(geCategoryItem['categoryId'])}>show</button> : ''}
                            </td>
                        );
                }
            });
    
            winLossBodyRows.push(
                <tr key={`result-body-category-${cidx}`}>
                    {row}
                 </tr>
            );
        }

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
            
            <div style={{background: "#E9E9E9",borderBottom: "0",padding:"8px",display:"flex",border:"1px solid #d5d5d5"}}>
                <button type="button" className="btn btn-light" 
                        style={props.TurnOverBtnDisabled ? {background:"black",color:"white"} : null}
                        onClick={() => props.SetShowDataBtn(props.TurnOverBtnDisabled,props.WinLossBtnDisabled)} 
                        disabled={props.TurnOverBtnDisabled}>TurnOver(ValidBet)</button>
                <button type="button" className="btn btn-light" 
                        style={props.WinLossBtnDisabled ? {background:"black",color:"white"} : null}
                        onClick={() => props.SetShowDataBtn(props.TurnOverBtnDisabled,props.WinLossBtnDisabled)} 
                        disabled={props.WinLossBtnDisabled}>WinLoss</button>
                <button type="button" className="btn btn-approved" 
                        style={{position:"absolute",right:"23px"}}
                        onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} 
                        disabled={props.ExportBtnDisabled}>Export Excel</button>
            </div>

            <table className="table table-striped" hidden={!props.TurnOverBtnDisabled}>
                <thead>
                    <tr>
                    <th colSpan="2" style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3",background:"#FFFF45",color:"#F00000"}}>TurnOver(ValidBet)</th>
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
            </table>

            <table className="table table-striped" hidden={!props.WinLossBtnDisabled}>
                <thead>
                    <tr>
                        <th colSpan="2" style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3",background:"#FFFF45",color:"#F00000"}}>WinLoss</th>
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