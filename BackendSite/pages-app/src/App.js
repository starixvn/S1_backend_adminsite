if(process.env.NODE_ENV!=='production'){
  Promise.all([
    import('react-dom'),
    import('react'),
    import('./component/dropdownBox'),
    import('./UserDefine'),
  ]).then(([ReactDOM, React, elem, UserDefine])=>{
    const pages = UserDefine.pages;

    const getCssItems = (cssList, pageName)=>{
      return cssList.map((cssPath, idx)=>{
          return <link key={`css-${pageName}-${idx}`} rel="stylesheet" type="text/css" href={cssPath} />
        });
    }

    class PageHandler extends React.Component{
      constructor(props){
        super(props)
        const defaultPage = UserDefine.defaultPage;
        let cssItems =  pages[defaultPage].css ? getCssItems(pages[defaultPage].css, defaultPage) : [];
        
        this.state = {func:defaultPage, css:cssItems}
        this.RenderSelectedPage(defaultPage)
      }
      OnPageClick=(PageName)=>{
        let cssItems = pages[PageName].css ? getCssItems(pages[PageName].css, PageName) : [];

        this.setState({func:PageName, css: cssItems}) 
        this.RenderSelectedPage(PageName)
      }
      RenderSelectedPage=(PageName)=>{
        ReactDOM.render(pages[PageName].comp, document.getElementById('content'))
      }
      render(){
        return (
          <>
            {this.state.css}
            {pages[this.state.func].layout === UserDefine.layoutType.LAP ? (
              <>
                <div className="panel panel--search">
                  <div className="panel__row">
                    <elem.DropdownBox 
                      ColName="Function Page" 
                      Items={Object.keys(pages).map(k=>{return k})} 
                      OnSelectItem={this.OnPageClick} 
                      SelectValue={this.state.func}
                      />
                  </div>
                </div> 
                <hr />
              </>
              ) : (
                <N88DropDownBox 
                  columnTitle="Function Page"
                  options={Object.keys(pages).map(k=>{return {displayText:k, value:k}})}
                  selectedValue={this.state.func}
                  dataColumn="Currency"
                  onChange={this.OnPageClick}
                />
              )
            }
          </>
        )
      }
    }

    const N88DropDownBox = (props)=>{
      const onChange = (e)=>{
          if(props.onChange){
              props.onChange(e.target.value, props.dataColumn);
          }
      }
      let optionRows = props.options.map(item=>{
          return(
              <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>{item.displayText}</option>
          )
      });
      return(
        <div className="filter">
            <div className="input-item">
                <label className="label">{props.columnTitle}</label>
                <div className="field">
                <select className="select-field" defaultValue={props.selectedValue} onChange={onChange}>
                    {optionRows}
                </select>
                </div>
            </div>
        </div>
      )
    }
    
    ReactDOM.render(<PageHandler />, document.getElementById('PageHandler'));
  });
}